using GmodAddonCompressor.CustomExtensions;
using GmodAddonCompressor.Interfaces;
using GmodAddonCompressor.Properties;
using GmodAddonCompressor.Systems;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class LUAEdit : ICompress
    {
        private readonly string _scriptCompact;
        private readonly string _scriptCommentsRemover;
        private readonly string _initScriptsCode;
        private readonly ILogger _logger = LogSystem.CreateLogger<LUAEdit>();

        public LUAEdit()
        {
            _scriptCompact = Encoding.UTF8.GetString(Resources.script_compact);
            _scriptCommentsRemover = Encoding.UTF8.GetString(Resources.script_comments_remover);
            _initScriptsCode = @"
                function LuaMinifer(luaCode)
                    luaCode = LuaCommentRemover(luaCode)
                    luaCode = LuaCompact(luaCode)
                    return luaCode
                end
            ";
        }

        public async Task Compress(string luaFilePath)
        {
            try
            {
                using (var luaMachine = new NLua.Lua())
                {
                    luaMachine.DoString(_scriptCompact);
                    luaMachine.DoString(_scriptCommentsRemover);
                    luaMachine.DoString(_initScriptsCode);

                    await Task.Yield();

                    var F_LuaMinifer = luaMachine["LuaMinifer"] as NLua.LuaFunction;
                    if (F_LuaMinifer != null)
                    {
                        string luaCode = await File.ReadAllTextAsync(luaFilePath);
                        string newLuaCode = (string)F_LuaMinifer.Call(luaCode).First();
                        if (!string.IsNullOrEmpty(newLuaCode))
                        {
                            await File.WriteAllTextAsync(luaFilePath, newLuaCode);
                            _logger.LogInformation($"Successful file compression: {luaFilePath.GAC_ToLocalPath()}");
                        }
                        else
                            _logger.LogError($"LUA compression failed: {luaFilePath.GAC_ToLocalPath()}");
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
