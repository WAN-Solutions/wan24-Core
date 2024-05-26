using System.Text;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class StringExtensions_Tests : TestBase
    {
        [TestMethod]
        public void UTF8_Tests()
        {
            Assert.AreEqual("test", "test".GetBytes().ToUtf8String());
        }

        [TestMethod]
        public void Parser_Tests()
        {
            Dictionary<string, string> data = new()
            {
                {"value", "test"},
                {"value2", " test "},
                {"html", "<"},
                {"json", "\\"},
                {"uri", "\""},
                {"items", "test2|test1"},
                {"tmpl", "%{_item}"},
                {"true", "1"},
                {"false", "0"},
                {"iftrue", "true (%{true})"},
                {"iffalse", "false (%{false})"},
            };
            Dictionary<string, string> test = new()
            {
                {"%{value}", "test"},
                {"%{value:sub(1)}", "est"},
                {"%{value:sub(1,2)}", "es"},
                {"%{value:left(1)}", "t"},
                {"%{value:right(2)}", "st"},
                {"%{value2:trim}", "test"},
                {"%{value:discard}", string.Empty},
                {"%{value:sub(1):discard}", string.Empty},
                {"%{html:escape_html}", "&lt;"},
                {"%{json:escape_json}", "\\\\"},
                {"%{uri:escape_uri}", "%22"},
                {"%{value:set(value2)}", "test"},
                {"%{value2}", "test"},
                {"%{:var($value)}", "test"},
                {"%{:item(0,test1,test2)}", "test1"},
                {"%{value:prepend(test1)}", "test1test"},
                {"%{value:append(test1)}", "testtest1"},
                {"%{value:insert(1,test)}", "ttestest"},
                {"%{value:remove(1,2)}", "tt"},
                {"%{:concat(test,test)}", "testtest"},
                {"%{:join(;,test,test)}", "test;test"},
                {"%{:math(+,1,1)}", "2"},
                {"%{:math(-,2,1)}", "1"},
                {"%{:math(*,2,2)}", "4"},
                {"%{:math(/,4,2)}", "2"},
                {"%{:math(%,3,2)}", "1"},
                {"%{:math(a,1,2,3)}", "2"},
                {"%{:math(i,1,2)}", "1"},
                {"%{:math(x,1,2)}", "2"},
                {"%{:math(r,1.234,2)}", "1.23"},
                {"%{:math(f,1.5,0)}", "1"},
                {"%{:math(c,1.5,0)}", "2"},
                {"%{:math(p,2,1)}", "2"},
                {"%{:math(=,1,1)}", "1"},
                {"%{:math(=,1,2)}", "0"},
                {"%{:math(s,1,0)}", "-1"},
                {"%{value:len}", "4"},
                {"%{items:count}", "2"},
                {"%{value:insert_item(0,$items)}", "test"},
                {"%{items:count:dummy}", "3"},
                {"%{items:remove_item(0):set(items):discard}", string.Empty},
                {"%{items:count:dummy()}", "2"},
                {"%{items:sort}", "test1|test2"},
                {"%{items:foreach($tmpl)}", "test2test1"},
                {"%{true:if($iftrue,$iffalse)}", "true (1)"},
                {"%{false:if($iftrue,$iffalse)}", "false (0)"},
                {"%{items:split(item):discard}", string.Empty},
                {"%{item0}", "test2"},
                {"%{item1}", "test1"},
                {"%{:range(0,3)}", "0|1|2"},
            };
            foreach (var kvp in test) Assert.AreEqual(kvp.Value, kvp.Key.Parse(data), kvp.Key);
        }

        [TestMethod]
        public void IsLike_Tests()
        {
            Assert.IsTrue("TEST".IsLike("test"));
            Assert.IsTrue("test".IsLike("test"));
            Assert.IsTrue("test".IsLike("TEST"));
            Assert.IsFalse("tset".IsLike("test"));
        }
    }
}
