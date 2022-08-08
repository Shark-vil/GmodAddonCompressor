using GmodAddonCompressor.CustomExtensions;
using GmodAddonCompressor.DataContexts;
using GmodAddonCompressor.Interfaces;
using GmodAddonCompressor.Properties;
using GmodAddonCompressor.Systems;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class LUAEdit : ICompress
    {
        private readonly string _scriptCompact;
        private readonly string _scriptCommentsRemover;
        private const string _mainDirectoryName = "Prometheus";
        private readonly string _prometheusFilePath;
        private string _mainDirectoryPath;
        private readonly ILogger _logger = LogSystem.CreateLogger<LUAEdit>();

        public LUAEdit()
        {
            _scriptCompact = Encoding.UTF8.GetString(Resources.script_compact);
            _scriptCommentsRemover = Encoding.UTF8.GetString(Resources.script_comments_remover);

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _mainDirectoryPath = Path.Combine(baseDirectory, _mainDirectoryName);

            if (!Directory.Exists(_mainDirectoryPath))
            {
                string zipResourcePath = Path.Combine(baseDirectory, _mainDirectoryName + ".zip");

                if (!File.Exists(zipResourcePath))
                    File.WriteAllBytes(zipResourcePath, Resources.Prometheus);

                ZipFile.ExtractToDirectory(zipResourcePath, baseDirectory);
                File.Delete(zipResourcePath);
            }

            _prometheusFilePath = Path.Combine(_mainDirectoryPath, "cli.lua");
        }

        public async Task Compress(string luaFilePath)
        {
            long oldFileSize = new FileInfo(luaFilePath).Length;

            await CompressProcess(luaFilePath);

            long newFileSize = new FileInfo(luaFilePath).Length;

            if (newFileSize < oldFileSize)
                _logger.LogInformation($"Successful file compression: {luaFilePath.GAC_ToLocalPath()}");
            else
                _logger.LogError($"LUA compression failed: {luaFilePath.GAC_ToLocalPath()}");
        }

        private async Task CompressProcess(string luaFilePath)
        {
            try
            {
                using (var luaMachine = new NLua.Lua())
                {
                    luaMachine.DoString(_scriptCompact);
                    luaMachine.DoString(_scriptCommentsRemover);

                    await Task.Yield();

                    var F_LuaCommentRemover = luaMachine["LuaCommentRemover"] as NLua.LuaFunction;
                    var F_LuaCompact = luaMachine["LuaCompact"] as NLua.LuaFunction;

                    if (F_LuaCompact != null && F_LuaCommentRemover != null)
                    {
                        string luaCode = await File.ReadAllTextAsync(luaFilePath);
                        string newLuaCode = (string)F_LuaCommentRemover.Call(luaCode).First();

                        if (string.IsNullOrEmpty(newLuaCode))
                            return;

                        await File.WriteAllTextAsync(luaFilePath, newLuaCode);

                        string tempLuaFilePath = luaFilePath + "____TEMP.lua";

                        if (LuaContext.ChangeOriginalCodeToMinimalistic)
                        {
                            try
                            {
                                using (var prometheusLuaMachine = new NLua.Lua())
                                {
                                    prometheusLuaMachine.DoString($"arg = {{ '{luaFilePath.Replace("\\", "\\\\")}', '--preset', 'Minify', '--out', '{tempLuaFilePath.Replace("\\", "\\\\")}' }} ");
                                    prometheusLuaMachine.DoFile(_prometheusFilePath);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex.ToString());
                            }
                        }

                        if (File.Exists(tempLuaFilePath))
                        {
                            File.Delete(luaFilePath);
                            File.Copy(tempLuaFilePath, luaFilePath);
                            File.Delete(tempLuaFilePath);

                            luaCode = await File.ReadAllTextAsync(luaFilePath);
                            newLuaCode = (string)F_LuaCommentRemover.Call(luaCode).First();
                            await File.WriteAllTextAsync(luaFilePath, newLuaCode);
                        }
                        else
                        {
                            newLuaCode = (string)F_LuaCompact.Call(newLuaCode).First();

                            if (string.IsNullOrEmpty(newLuaCode))
                                return;

                            await File.WriteAllTextAsync(luaFilePath, newLuaCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }
    }
}
