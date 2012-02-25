using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Winslew.Api
{
    public class Pastebin
    {
        bool SendToPastebin(string text, string name, string subdomain, bool isPrivate, string expiration, string format)
        {
            return true;
        }

        public Dictionary<string, string> AvailableFormats;
        public List<string> FormatNames;

        public Pastebin()
        {
            AvailableFormats = new Dictionary<string, string>();
            AvailableFormats.Add("", "No highlighting");
            AvailableFormats.Add("abap","ABAP");
            AvailableFormats.Add("actionscript","ActionScript");
            AvailableFormats.Add("actionscript3","ActionScript3");
            AvailableFormats.Add("ada","Ada");
            AvailableFormats.Add("apache","ApacheLog");
            AvailableFormats.Add("applescript","AppleScript");
            AvailableFormats.Add("apt_sources","APTSources");
            AvailableFormats.Add("asm","ASM(NASM)");
            AvailableFormats.Add("asp","ASP");
            AvailableFormats.Add("autoit","AutoIt");
            AvailableFormats.Add("avisynth","Avisynth");
            AvailableFormats.Add("bash","Bash");
            AvailableFormats.Add("basic4gl","Basic4GL");
            AvailableFormats.Add("bibtex","BibTeX");
            AvailableFormats.Add("blitzbasic","BlitzBasic");
            AvailableFormats.Add("bnf","BNF");
            AvailableFormats.Add("boo","BOO");
            AvailableFormats.Add("bf","BrainFuck");
            AvailableFormats.Add("c","C");
            AvailableFormats.Add("c_mac","CforMacs");
            AvailableFormats.Add("cill","CIntermediateLanguage");
            AvailableFormats.Add("csharp","C#");
            AvailableFormats.Add("cpp","C++");
            AvailableFormats.Add("caddcl","CADDCL");
            AvailableFormats.Add("cadlisp","CADLisp");
            AvailableFormats.Add("cfdg","CFDG");
            AvailableFormats.Add("klonec","CloneC");
            AvailableFormats.Add("klonecpp","CloneC++");
            AvailableFormats.Add("cmake","CMake");
            AvailableFormats.Add("cobol","COBOL");
            AvailableFormats.Add("cfm","ColdFusion");
            AvailableFormats.Add("css","CSS");
            AvailableFormats.Add("d","D");
            AvailableFormats.Add("dcs","DCS");
            AvailableFormats.Add("delphi","Delphi");
            AvailableFormats.Add("dff","Diff");
            AvailableFormats.Add("div","DIV");
            AvailableFormats.Add("dos","DOS");
            AvailableFormats.Add("dot","DOT");
            AvailableFormats.Add("eiffel","Eiffel");
            AvailableFormats.Add("email","Email");
            AvailableFormats.Add("erlang","Erlang");
            AvailableFormats.Add("fo","FOLanguage");
            AvailableFormats.Add("fortran","Fortran");
            AvailableFormats.Add("freebasic","FreeBasic");
            AvailableFormats.Add("gml","GameMaker");
            AvailableFormats.Add("genero","Genero");
            AvailableFormats.Add("gettext","GetText");
            AvailableFormats.Add("groovy","Groovy");
            AvailableFormats.Add("haskell","Haskell");
            AvailableFormats.Add("hq9plus","HQ9Plus");
            AvailableFormats.Add("html4strict","HTML");
            AvailableFormats.Add("idl","IDL");
            AvailableFormats.Add("ini","INIfile");
            AvailableFormats.Add("inno","InnoScript");
            AvailableFormats.Add("intercal","INTERCAL");
            AvailableFormats.Add("io","IO");
            AvailableFormats.Add("java","Java");
            AvailableFormats.Add("java5","Java5");
            AvailableFormats.Add("javascript","JavaScript");
            AvailableFormats.Add("kixtart","KiXtart");
            AvailableFormats.Add("latex","Latex");
            AvailableFormats.Add("lsl2","LindenScripting");
            AvailableFormats.Add("lisp","Lisp");
            AvailableFormats.Add("locobasic","LocoBasic");
            AvailableFormats.Add("lolcode","LOLCode");
            AvailableFormats.Add("lotusformulas","LotusFormulas");
            AvailableFormats.Add("lotusscript","LotusScript");
            AvailableFormats.Add("lscript","LScript");
            AvailableFormats.Add("lua","Lua");
            AvailableFormats.Add("m68k","M68000Assembler");
            AvailableFormats.Add("make","Make");
            AvailableFormats.Add("matlab","MatLab");
            AvailableFormats.Add("mirc","mIRC");
            AvailableFormats.Add("modula3","Modula3");
            AvailableFormats.Add("mpasm","MPASM");
            AvailableFormats.Add("mxml","MXML");
            AvailableFormats.Add("mysql","MySQL");
            AvailableFormats.Add("text","None");
            AvailableFormats.Add("nsis","NullSoftInstaller");
            AvailableFormats.Add("oberon2","Oberon2");
            AvailableFormats.Add("objc","ObjectiveC");
            AvailableFormats.Add("ocaml-brief","OCalmBrief");
            AvailableFormats.Add("ocaml","OCaml");
            AvailableFormats.Add("glsl","OpenGLShading");
            AvailableFormats.Add("oobas","OpenofficeBASIC");
            AvailableFormats.Add("oracle11","Oracle11");
            AvailableFormats.Add("oracle8","Oracle8");
            AvailableFormats.Add("pascal","Pascal");
            AvailableFormats.Add("pawn","PAWN");
            AvailableFormats.Add("per","Per");
            AvailableFormats.Add("perl","Perl");
            AvailableFormats.Add("php","PHP");
            AvailableFormats.Add("php-brief","PHPBrief");
            AvailableFormats.Add("pic16","Pic16");
            AvailableFormats.Add("pixelbender","PixelBender");
            AvailableFormats.Add("plsql","PL/SQL");
            AvailableFormats.Add("povray","POV-Ray");
            AvailableFormats.Add("powershell","PowerShell");
            AvailableFormats.Add("progress","Progress");
            AvailableFormats.Add("prolog","Prolog");
            AvailableFormats.Add("properties","Properties");
            AvailableFormats.Add("providex","ProvideX");
            AvailableFormats.Add("python","Python");
            AvailableFormats.Add("qbasic","QBasic");
            AvailableFormats.Add("rails","Rails");
            AvailableFormats.Add("rebol","REBOL");
            AvailableFormats.Add("reg","REG");
            AvailableFormats.Add("robots","Robots");
            AvailableFormats.Add("ruby","Ruby");
            AvailableFormats.Add("gnuplot","RubyGnuplot");
            AvailableFormats.Add("sas","SAS");
            AvailableFormats.Add("scala","Scala");
            AvailableFormats.Add("scheme","Scheme");
            AvailableFormats.Add("scilab","Scilab");
            AvailableFormats.Add("sdlbasic","SdlBasic");
            AvailableFormats.Add("smalltalk","Smalltalk");
            AvailableFormats.Add("smarty","Smarty");
            AvailableFormats.Add("sql","SQL");
            AvailableFormats.Add("tsql","T-SQL");
            AvailableFormats.Add("tcl","TCL");
            AvailableFormats.Add("teraterm","TeraTerm");
            AvailableFormats.Add("thinbasic","thinBasic");
            AvailableFormats.Add("typoscript","TypoScript");
            AvailableFormats.Add("unreal","unrealScript");
            AvailableFormats.Add("vbnet","VB.NET");
            AvailableFormats.Add("verilog","VeriLog");
            AvailableFormats.Add("vhdl","VHDL");
            AvailableFormats.Add("vim","VIM");
            AvailableFormats.Add("visualprolog","VisualProLog");
            AvailableFormats.Add("vb","VisualBasic");
            AvailableFormats.Add("visualfoxpro","VisualFoxPro");
            AvailableFormats.Add("whitespace","WhiteSpace");
            AvailableFormats.Add("whois","WHOIS");
            AvailableFormats.Add("winbatch","WinBatch");
            AvailableFormats.Add("xml","XML");
            AvailableFormats.Add("xorg_conf","XorgConfig");
            AvailableFormats.Add("xpp","XPP");
            AvailableFormats.Add("z80","Z80Assembler");

            FormatNames = new List<string>();
            foreach(string format in AvailableFormats.Values)
            {
                FormatNames.Add(format);
            }
        }

        public Result AddToPastebin(string text, string title, string email,string subdomain,bool privat,string expireDate,string format) {
            Result returnValue = new Result();
            Response result = new Response();
            string isPrivate = "1";
            if (!privat)
            {
                isPrivate = "0";
            }
            try
            {
                result = HttpCommunications.SendPostRequest(@"http://pastebin.com/api_public.php", new
                {
                    paste_code = text,
                    paste_name = title,
                    paste_email  = email,
                    paste_subdomain = subdomain,
                    paste_private = isPrivate,
                    paste_expire_date = expireDate,
                    paste_format = format
                }, false);
            }
            catch (Exception exp)
            {
                returnValue.Success = false;
                returnValue.ErrorText = "General sending error: " + exp.Message;
                return returnValue;
            }


                if (!result.Content.StartsWith("http"))
                {
                    returnValue.Success = false;
                    returnValue.ErrorText = "Pastebin says: " + result.Content;
                }
                else
                {
                    returnValue.Success = true;
                    returnValue.ErrorText = "";
                    returnValue.PastebinUrl = result.Content;
                }


            return returnValue;
        }

        public class Result
        {
            public bool Success;
            public string ErrorText;
            public string PastebinUrl;

            public Result()
            {
                Success = false;
                ErrorText = "Unknown error";
                PastebinUrl = "";
            }
        }
    }
}
