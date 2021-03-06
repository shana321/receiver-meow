function getInfo(av)
    local html = httpGet("https://www.jijidown.com/api/v1/video/get_info?id="..av)
    if not html then return "查找失败" end
    local j,r,e = jsonDecode(html)
    if j.title == "正在加载数据..." then
        return getInfo(av)
    end
    if not r or j.upid == -1 then return "数据解析失败啦" end
    image = j.img and image("http:"..j.img)
    return (image and image.."\r\n" or "")..
    "av"..av..",标题："..j.title..
    "\r\n"..j.desc:gsub("<br/>","\r\n")..
    "\r\n分区："..(j.sort and j.sort..(j.subsort and "->"..j.subsort or "") or "")
end

if message:match("av(%d+)") then
    print(at(fromqq))
    print(getInfo(message:match("av(%d+)")))
end
