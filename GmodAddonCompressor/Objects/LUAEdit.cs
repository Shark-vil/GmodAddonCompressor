using GmodAddonCompressor.CustomExtensions;
using GmodAddonCompressor.DataContexts;
using GmodAddonCompressor.Interfaces;
using GmodAddonCompressor.Properties;
using GmodAddonCompressor.Systems;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class LUAEdit : ICompress
    {
        private readonly string _scriptCompact;
        private readonly string _scriptCommentsRemover;
        private static readonly Regex _regexUtf8 = new Regex("[^\x00-\x7F]");
        //private static readonly Regex _regexUnicode = new Regex("[^\u0000-\u007F]+");
        private const string _mainDirectoryNamePrometheus = "Prometheus";
        private const string _mainDirectoryNameGLuaFixer = "GLuaFixer";
        private readonly string _prometheusFilePath;
        private readonly string _gLuaFixerFilePath;
        private string _mainDirectoryPrometheusPath;
        private string _mainDirectoryGLuaFixerPath;
        private readonly ILogger _logger = LogSystem.CreateLogger<LUAEdit>();

        public LUAEdit()
        {
            _scriptCompact = Encoding.UTF8.GetString(Resources.script_compact);
            _scriptCommentsRemover = Encoding.UTF8.GetString(Resources.script_comments_remover);

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _mainDirectoryPrometheusPath = Path.Combine(baseDirectory, _mainDirectoryNamePrometheus);
            _mainDirectoryGLuaFixerPath = Path.Combine(baseDirectory, _mainDirectoryNameGLuaFixer);

            if (!Directory.Exists(_mainDirectoryPrometheusPath))
            {
                string zipResourcePath = Path.Combine(baseDirectory, _mainDirectoryNamePrometheus + ".zip");

                if (!File.Exists(zipResourcePath))
                    File.WriteAllBytes(zipResourcePath, Resources.Prometheus);

                ZipFile.ExtractToDirectory(zipResourcePath, baseDirectory);
                File.Delete(zipResourcePath);
            }

            if (!Directory.Exists(_mainDirectoryGLuaFixerPath))
            {
                Directory.CreateDirectory(_mainDirectoryGLuaFixerPath);
    
                string zipResourcePath = Path.Combine(baseDirectory, _mainDirectoryNameGLuaFixer + ".zip");

                if (!File.Exists(zipResourcePath))
                    File.WriteAllBytes(zipResourcePath, Resources.glualint);

                ZipFile.ExtractToDirectory(zipResourcePath, _mainDirectoryGLuaFixerPath);
                File.Delete(zipResourcePath);
            }

            _prometheusFilePath = Path.Combine(_mainDirectoryPrometheusPath, "cli.lua");
            _gLuaFixerFilePath = Path.Combine(_mainDirectoryGLuaFixerPath, "glualint.exe");
        }

        private static string Replace(Match match) => @"\\u" + ((int)match.Value[0]).ToString("x4");

        private static string EncodeUTF8(string unescaped) => _regexUtf8.Replace(unescaped, Replace);

        private static string DecodeUTF8(string escaped)
        {
            return Regex.Replace(escaped, @"\\\\[Uu]([0-9A-Fa-f]{4})", m => char.ToString(
                (char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)
                )
            );
        }

        public async Task Compress(string luaFilePath)
        {
            long oldFileSize = new FileInfo(luaFilePath).Length;

            string tempLuaFilePath = luaFilePath + "____TEMP.lua";
            File.Copy(luaFilePath, tempLuaFilePath);

            string luaCode = await File.ReadAllTextAsync(luaFilePath);
            byte[] encodeBytes = Encoding.Default.GetBytes(EncodeUTF8(luaCode));
            await File.WriteAllBytesAsync(luaFilePath, encodeBytes);

            await CompressProcess(luaFilePath);

            string luaCodeEncode = await File.ReadAllTextAsync(luaFilePath);
            await File.WriteAllTextAsync(luaFilePath, DecodeUTF8(luaCodeEncode));

            long newFileSize = new FileInfo(luaFilePath).Length;

            if (newFileSize < oldFileSize)
                _logger.LogInformation($"Successful file compression: {luaFilePath.GAC_ToLocalPath()}");
            else
            {
                _logger.LogError($"LUA compression failed: {luaFilePath.GAC_ToLocalPath()}");

                if (File.Exists(tempLuaFilePath))
                {
                    File.Delete(luaFilePath);
                    File.Copy(tempLuaFilePath, luaFilePath);
                }
            }

            if (File.Exists(tempLuaFilePath))
                File.Delete(tempLuaFilePath);
        }

        private async Task LuaFilePrettyPrint(string luaFilePath)
        {
            var glualintCmdProcess = new Process();
            glualintCmdProcess.StartInfo.FileName = _gLuaFixerFilePath;
            glualintCmdProcess.StartInfo.Arguments = $" --pretty-print-files \"{luaFilePath}\"";
            glualintCmdProcess.StartInfo.UseShellExecute = false;
            glualintCmdProcess.StartInfo.CreateNoWindow = true;
            glualintCmdProcess.StartInfo.RedirectStandardOutput = true;
            glualintCmdProcess.StartInfo.RedirectStandardError = true;
            glualintCmdProcess.OutputDataReceived += (sender, args) => _logger.LogDebug(args.Data);
            glualintCmdProcess.ErrorDataReceived += (sender, args) => _logger.LogDebug(args.Data);
            glualintCmdProcess.Start();
            glualintCmdProcess.BeginOutputReadLine();
            glualintCmdProcess.BeginErrorReadLine();

            //await Task.WhenAny(glualintCmdProcess.WaitForExitAsync(), Task.Delay(3000));

            await glualintCmdProcess.WaitForExitAsync();
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
                        await LuaFilePrettyPrint(luaFilePath);

                        string outLuaFilePath = luaFilePath + "____OUT.lua";

                        if (LuaContext.ChangeOriginalCodeToMinimalistic)
                        {
                            try
                            {
                                await File.WriteAllTextAsync(luaFilePath, newLuaCode, Encoding.ASCII);

                                using (var prometheusLuaMachine = new NLua.Lua())
                                {
                                    prometheusLuaMachine.DoString($"arg = {{ '{luaFilePath.Replace("\\", "\\\\")}', '--preset', 'Minify', '--out', '{outLuaFilePath.Replace("\\", "\\\\")}' }} ");
                                    prometheusLuaMachine.DoFile(_prometheusFilePath);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex.ToString());
                            }
                        }

                        if (File.Exists(outLuaFilePath))
                        {
                            File.Delete(luaFilePath);
                            File.Copy(outLuaFilePath, luaFilePath);
                            File.Delete(outLuaFilePath);

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
