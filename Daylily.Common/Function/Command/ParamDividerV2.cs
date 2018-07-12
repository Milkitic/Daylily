﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Daylily.Common.Assist;
using Daylily.Common.Utils;
using Daylily.Common.Utils.LogUtils;

namespace Daylily.Common.Function.Command
{
    class ParamDividerV2 : IParamDivider
    {
        public string CommandName { get; private set; }
        public string Parameter { get; private set; }

        public ConcurrentDictionary<string, string> Parameters { get; } = new ConcurrentDictionary<string, string>();
        public ConcurrentDictionary<string, string> Switches { get; } = new ConcurrentDictionary<string, string>();
        public List<string> SimpleParams { get; set; }

        private static readonly char[] Quote = { '\'', '\"' };
        public bool TryDivide(string fullCmd)
        {
            CommandName = fullCmd.Split(' ')[0].Trim();
            Parameter = fullCmd.IndexOf(" ", StringComparison.Ordinal) == -1
                ? ""
                : fullCmd.Substring(fullCmd.IndexOf(" ", StringComparison.Ordinal) + 1,
                    fullCmd.Length - CommandName.Length - 1).Trim();
            List<string> splitedParam = new List<string>();
            SimpleParams = Parameter.Split(' ').ToList();
            if (Parameter == "") return false;
            try
            {
                splitedParam.AddRange(Parameter.Split(' '));
                foreach (var item in splitedParam)
                {
                    if (Quote.Any(q => ContainsChar(q, item)))
                    {
                        throw new ArgumentException();
                    }
                }

                bool combined = true;
                foreach (var item in Quote)
                {
                    for (int i = 0; i < splitedParam.Count - 1; i++)
                    {
                        string cur = splitedParam[i], next = splitedParam[i + 1];

                        if (cur.StartsWith(item) && !cur.EndsWith(item))
                        {
                            combined = false;
                            splitedParam[i] = cur + " " + next;
                            splitedParam.Remove(next);
                            if (splitedParam[i].EndsWith(item))
                                combined = true;
                            i--;
                        }
                    }
                    if (!combined) throw new ArgumentException("Expect '" + item + "'.");
                }

                string tmpKey = null, tmpValue = null;
                bool isLastKeyOrValue = true;

                splitedParam.Add("-");
                foreach (var item in splitedParam)
                {
                    if (item.StartsWith('-'))
                    {
                        if (tmpKey != null)
                        {
                            Switches.GetOrAdd(tmpKey, tmpKey);
                        }

                        tmpKey = item.Remove(0, 1);
                        isLastKeyOrValue = true;
                    }
                    else
                    {
                        if (!isLastKeyOrValue)
                            throw new ArgumentException("Expect key.");
                        foreach (var q in Quote)
                        {
                            tmpValue = item.Trim(q);
                        }

                        Parameters.GetOrAdd(tmpKey, tmpValue);
                        tmpKey = null;
                        tmpValue = null;
                        isLastKeyOrValue = false;
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Debug(ex.Message);
                return false;
            }

            return true;
        }

        private bool ContainsChar(char ch, string str)
        {
            char[] cs = str.ToCharArray();
            for (int i = 1; i < cs.Length - 1; i++)
            {
                if (cs[i] == ch)
                    return true;
            }

            return false;
        }
    }
}

