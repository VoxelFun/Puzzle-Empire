using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Parser{

    readonly StringReader reader;
    readonly string @string;

    public Parser(string s) {
        reader = new StringReader(s);
        @string = s;
    }

    /// <summary>
    /// Doesn't skip '\n'
    /// </summary>
    /// <returns></returns>
    public char GetChar() {
        return (char)reader.Read();
    }

    public int GetInt() {
        int result = 0;
        while (!IsEnd()) {
            char c = GetChar();
            if (!char.IsNumber(c))
                break;
            result = result * 10 + (c - 48);
        }
        return result;
    }

    public string GetTheRest() {
        return reader.ReadToEnd();
    }

    public string GetTheRestOfLine() {
        return reader.ReadLine();
    }

    public string GetSentence(char breakChar) {
        StringWriter writer = new StringWriter();
        while (!IsEnd()) {
            char c = GetChar();
            if (c == breakChar)
                break;
            writer.Write(c);
        }
        return writer.ToString();
    }

    public string GetSentenceToLast(char breakChar) {
        int id = @string.LastIndexOf(breakChar);
        if (id < 0)
            return "";
        for (int i = 0; i <= id; i++)
            GetChar();
        return @string.Substring(0, id);
    }

    public string GetWord() {
        return GetSentence(' ');
    }

    public bool IsEnd() {
        return reader.Peek() < 0;
    }


    public static int Count(string @string, char @char) {
        int result = 0;
        foreach (char character in @string) {
            if (character == @char)
                result++;
        }
        return result;
    }

}
