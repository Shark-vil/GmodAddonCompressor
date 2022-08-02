using GmodAddonCompressor.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmodAddonCompressor.Objects
{
    internal class LUAEdit : IDisposable
    {
        private readonly string _scriptCompact;
        private readonly string _scriptCommentsRemover;
        private readonly string _initScriptsCode;
        private NLua.Lua _luaMachine;

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

            _luaMachine = new NLua.Lua();
            _luaMachine.DoString(_scriptCompact);
            _luaMachine.DoString(_scriptCommentsRemover);
            _luaMachine.DoString(_initScriptsCode);
        }

        public void Dispose()
        {
            _luaMachine.Dispose();
        }

        internal async Task LuaCompress(string luaFilePath)
        {
            try
            {
                var F_LuaMinifer = _luaMachine["LuaMinifer"] as NLua.LuaFunction;
                string luaCode = await File.ReadAllTextAsync(luaFilePath);
                string newLuaCode = (string)F_LuaMinifer.Call(luaCode).First();
                if (!string.IsNullOrEmpty(newLuaCode))
                {
                    await File.WriteAllTextAsync(luaFilePath, newLuaCode);
                    Console.WriteLine($"Optimization LUA: {luaFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
