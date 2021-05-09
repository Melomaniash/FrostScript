﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript
{
    public class ParseException : Exception
    {
        public int Line { get; }
        public int CharacterPos { get; }
        public override string Message { get; }

        public ParseException(int line, int character, string message)
        {
            Line = line;
            CharacterPos = character;
            Message = message;
        }
    }
}
