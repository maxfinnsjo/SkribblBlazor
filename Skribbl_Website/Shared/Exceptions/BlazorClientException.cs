﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Skribbl_Website.Shared.Exceptions
{
    public class BlazorClientException : Exception
    {
        public BlazorClientException(Exception inner) : base("",inner)
        {

        }
    }
}
