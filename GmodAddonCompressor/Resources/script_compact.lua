function LuaCompact(lua_code)
	local new_lua_code = ''

	for code_line in lua_code:gmatch('[^\r\n]+') do
		code_line = code_line:gsub('^%s*(.-)%s*$', '%1')
		if #code_line ~= 0 then
			new_lua_code = new_lua_code .. code_line .. ' '
		end
	end

	return new_lua_code
end