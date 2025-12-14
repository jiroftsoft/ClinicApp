using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Parser برای Template هوشمند
    /// پشتیبانی از متغیرها، شرطی‌سازی و حلقه‌ها
    /// </summary>
    public class SmartTemplateParser
    {
        private readonly string _template;
        private int _position;
        private readonly List<TemplateNode> _nodes;

        public SmartTemplateParser(string template)
        {
            _template = template ?? string.Empty;
            _position = 0;
            _nodes = new List<TemplateNode>();
        }

        /// <summary>
        /// Parse کردن Template به AST (Abstract Syntax Tree)
        /// </summary>
        public List<TemplateNode> Parse()
        {
            _nodes.Clear();
            _position = 0;

            while (_position < _template.Length)
            {
                // جستجوی دستورات شرطی {{#if}}
                if (TryParseConditional())
                {
                    continue;
                }

                // جستجوی حلقه {{#for}}
                if (TryParseLoop())
                {
                    continue;
                }

                // جستجوی متغیر ساده {{VariableName}}
                if (TryParseVariable())
                {
                    continue;
                }

                // متن عادی
                if (TryParseText())
                {
                    continue;
                }

                // اگر هیچکدام match نشد، یک کاراکتر جلو برو
                _position++;
            }

            return _nodes;
        }

        /// <summary>
        /// Parse کردن دستور شرطی {{#if Condition}} ... {{#else}} ... {{/if}}
        /// </summary>
        private bool TryParseConditional()
        {
            var ifPattern = @"\{\{#if\s+([^}]+)\}\}";
            var match = Regex.Match(_template.Substring(_position), ifPattern, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return false;
            }

            var condition = match.Groups[1].Value.Trim();
            var ifStart = _position + match.Index;
            var ifEnd = ifStart + match.Length;

            // پیدا کردن {{/if}}
            var ifEndPattern = @"\{\{/if\}\}";
            var endMatch = Regex.Match(_template.Substring(ifEnd), ifEndPattern, RegexOptions.IgnoreCase);

            if (!endMatch.Success)
            {
                // اگر {{/if}} پیدا نشد، به عنوان متن عادی treat می‌کنیم
                return false;
            }

            var ifCloseEnd = ifEnd + endMatch.Index + endMatch.Length;

            // پیدا کردن {{#else}} (اختیاری)
            var elsePattern = @"\{\{#else\}\}";
            var elseMatch = Regex.Match(_template.Substring(ifEnd, ifCloseEnd - ifEnd - endMatch.Length), elsePattern, RegexOptions.IgnoreCase);

            string trueContent = string.Empty;
            string falseContent = string.Empty;

            if (elseMatch.Success)
            {
                var elseStart = ifEnd + elseMatch.Index;
                var elseEnd = elseStart + elseMatch.Length;
                trueContent = _template.Substring(ifEnd, elseStart - ifEnd);
                falseContent = _template.Substring(elseEnd, ifCloseEnd - elseEnd - endMatch.Length);
            }
            else
            {
                trueContent = _template.Substring(ifEnd, ifCloseEnd - ifEnd - endMatch.Length);
            }

            // Parse کردن محتوای true و false
            var trueParser = new SmartTemplateParser(trueContent);
            var falseParser = new SmartTemplateParser(falseContent);

            var conditionalNode = new ConditionalNode
            {
                Condition = condition,
                TrueContent = trueParser.Parse(),
                FalseContent = falseParser.Parse()
            };

            _nodes.Add(conditionalNode);
            _position = ifCloseEnd;

            return true;
        }

        /// <summary>
        /// Parse کردن حلقه {{#for Collection}} ... {{/for}}
        /// </summary>
        private bool TryParseLoop()
        {
            var forPattern = @"\{\{#for\s+([^}]+)\}\}";
            var match = Regex.Match(_template.Substring(_position), forPattern, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return false;
            }

            var collectionName = match.Groups[1].Value.Trim();
            var forStart = _position + match.Index;
            var forEnd = forStart + match.Length;

            // پیدا کردن {{/for}}
            var forEndPattern = @"\{\{/for\}\}";
            var endMatch = Regex.Match(_template.Substring(forEnd), forEndPattern, RegexOptions.IgnoreCase);

            if (!endMatch.Success)
            {
                return false;
            }

            var forCloseEnd = forEnd + endMatch.Index + endMatch.Length;
            var loopContent = _template.Substring(forEnd, forCloseEnd - forEnd - endMatch.Length);

            // Parse کردن محتوای حلقه
            var contentParser = new SmartTemplateParser(loopContent);

            var loopNode = new LoopNode
            {
                CollectionName = collectionName,
                LoopContent = contentParser.Parse()
            };

            _nodes.Add(loopNode);
            _position = forCloseEnd;

            return true;
        }

        /// <summary>
        /// Parse کردن متغیر ساده {{VariableName}}
        /// </summary>
        private bool TryParseVariable()
        {
            var variablePattern = @"\{\{([^#/][^}]*)\}\}";
            var match = Regex.Match(_template.Substring(_position), variablePattern);

            if (!match.Success)
            {
                return false;
            }

            var variableName = match.Groups[1].Value.Trim();
            var variableNode = new VariableNode
            {
                VariableName = variableName
            };

            _nodes.Add(variableNode);
            _position += match.Index + match.Length;

            return true;
        }

        /// <summary>
        /// Parse کردن متن عادی
        /// </summary>
        private bool TryParseText()
        {
            if (_position >= _template.Length)
            {
                return false;
            }

            // پیدا کردن اولین {{ یا پایان رشته
            var nextOpen = _template.IndexOf("{{", _position, StringComparison.Ordinal);
            var textEnd = nextOpen == -1 ? _template.Length : nextOpen;

            if (textEnd > _position)
            {
                var text = _template.Substring(_position, textEnd - _position);
                if (!string.IsNullOrEmpty(text))
                {
                    var textNode = new TextNode
                    {
                        Text = text
                    };
                    _nodes.Add(textNode);
                }
                _position = textEnd;
                return true;
            }

            return false;
        }
    }

    #region Template Node Classes

    /// <summary>
    /// Base class برای تمام Node های Template
    /// </summary>
    public abstract class TemplateNode
    {
        public abstract string Render(Dictionary<string, object> variables);
    }

    /// <summary>
    /// Node برای متن عادی
    /// </summary>
    public class TextNode : TemplateNode
    {
        public string Text { get; set; }

        public override string Render(Dictionary<string, object> variables)
        {
            return Text ?? string.Empty;
        }
    }

    /// <summary>
    /// Node برای متغیر
    /// </summary>
    public class VariableNode : TemplateNode
    {
        /// <summary>
        /// آیا این متغیر باید HTML Encode شود؟
        /// متغیرهای خاص که HTML هستند (مثل UnsubscribeUrl) نباید Encode شوند
        /// </summary>
        public bool ShouldEncodeHtml { get; set; } = true;

        public string VariableName { get; set; }

        public override string Render(Dictionary<string, object> variables)
        {
            if (variables == null || string.IsNullOrWhiteSpace(VariableName))
            {
                return string.Empty;
            }

            // جستجوی case-insensitive
            var key = variables.Keys.FirstOrDefault(k => 
                string.Equals(k, VariableName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (key != null)
            {
                var value = variables[key];
                var stringValue = value?.ToString() ?? string.Empty;

                // 🔒 Security: HTML Encode برای متغیرها (مگر اینکه Explicit AllowHtml داشته باشیم)
                // متغیرهای خاص که HTML هستند نباید Encode شوند
                var htmlVariables = new[] { "UnsubscribeUrl", "Content", "HtmlContent" };
                var shouldEncode = ShouldEncodeHtml && 
                                   !htmlVariables.Any(hv => string.Equals(hv, VariableName.Trim(), StringComparison.OrdinalIgnoreCase));

                if (shouldEncode && !string.IsNullOrEmpty(stringValue))
                {
                    return System.Web.HttpUtility.HtmlEncode(stringValue);
                }

                return stringValue;
            }

            // ✅ Test Case: Missing Variable - باید Empty بشه، نه Exception
            return string.Empty;
        }
    }

    /// <summary>
    /// Node برای شرطی‌سازی
    /// </summary>
    public class ConditionalNode : TemplateNode
    {
        public string Condition { get; set; }
        public List<TemplateNode> TrueContent { get; set; } = new List<TemplateNode>();
        public List<TemplateNode> FalseContent { get; set; } = new List<TemplateNode>();

        public override string Render(Dictionary<string, object> variables)
        {
            if (EvaluateCondition(Condition, variables))
            {
                return RenderNodes(TrueContent, variables);
            }
            else
            {
                return RenderNodes(FalseContent, variables);
            }
        }

        /// <summary>
        /// ارزیابی شرط
        /// </summary>
        private bool EvaluateCondition(string condition, Dictionary<string, object> variables)
        {
            if (string.IsNullOrWhiteSpace(condition))
            {
                return false;
            }

            condition = condition.Trim();

            // پشتیبانی از عملگرهای ==, !=, >, <, >=, <=
            var operators = new[] { "==", "!=", ">=", "<=", ">", "<" };
            string op = null;
            string left = null;
            string right = null;

            foreach (var opItem in operators)
            {
                var index = condition.IndexOf(opItem, StringComparison.OrdinalIgnoreCase);
                if (index > 0)
                {
                    op = opItem;
                    left = condition.Substring(0, index).Trim();
                    right = condition.Substring(index + opItem.Length).Trim();
                    break;
                }
            }

            // اگر عملگر پیدا نشد، بررسی می‌کنیم که آیا متغیر وجود دارد و مقدار truthy دارد
            if (op == null)
            {
                var value = GetVariableValue(condition, variables);
                return IsTruthy(value);
            }

            // ارزیابی با عملگر
            var leftValue = GetVariableValue(left, variables);
            var rightValue = GetVariableValue(right, variables);

            switch (op)
            {
                case "==":
                    return string.Equals(leftValue?.ToString(), rightValue?.ToString(), StringComparison.OrdinalIgnoreCase);
                case "!=":
                    return !string.Equals(leftValue?.ToString(), rightValue?.ToString(), StringComparison.OrdinalIgnoreCase);
                case ">":
                    return CompareValues(leftValue, rightValue) > 0;
                case "<":
                    return CompareValues(leftValue, rightValue) < 0;
                case ">=":
                    return CompareValues(leftValue, rightValue) >= 0;
                case "<=":
                    return CompareValues(leftValue, rightValue) <= 0;
                default:
                    return false;
            }
        }

        /// <summary>
        /// دریافت مقدار متغیر
        /// </summary>
        private object GetVariableValue(string variableName, Dictionary<string, object> variables)
        {
            if (string.IsNullOrWhiteSpace(variableName))
            {
                return null;
            }

            variableName = variableName.Trim();

            // اگر در quotes است، به عنوان string literal treat می‌کنیم
            if ((variableName.StartsWith("\"") && variableName.EndsWith("\"")) ||
                (variableName.StartsWith("'") && variableName.EndsWith("'")))
            {
                return variableName.Substring(1, variableName.Length - 2);
            }

            // جستجوی case-insensitive
            var key = variables?.Keys.FirstOrDefault(k => 
                string.Equals(k, variableName, StringComparison.OrdinalIgnoreCase));

            return key != null ? variables[key] : null;
        }

        /// <summary>
        /// بررسی truthy بودن مقدار
        /// </summary>
        private bool IsTruthy(object value)
        {
            if (value == null)
            {
                return false;
            }

            if (value is bool boolValue)
            {
                return boolValue;
            }

            if (value is string stringValue)
            {
                return !string.IsNullOrWhiteSpace(stringValue) && 
                       !string.Equals(stringValue, "false", StringComparison.OrdinalIgnoreCase) &&
                       !string.Equals(stringValue, "0", StringComparison.OrdinalIgnoreCase);
            }

            if (value is int intValue)
            {
                return intValue != 0;
            }

            return true;
        }

        /// <summary>
        /// مقایسه مقادیر
        /// </summary>
        private int CompareValues(object left, object right)
        {
            if (left == null && right == null) return 0;
            if (left == null) return -1;
            if (right == null) return 1;

            // تلاش برای تبدیل به عدد
            if (double.TryParse(left.ToString(), out var leftNum) && 
                double.TryParse(right.ToString(), out var rightNum))
            {
                return leftNum.CompareTo(rightNum);
            }

            // مقایسه string
            return string.Compare(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Render کردن لیست Node ها
        /// </summary>
        private string RenderNodes(List<TemplateNode> nodes, Dictionary<string, object> variables)
        {
            if (nodes == null || !nodes.Any())
            {
                return string.Empty;
            }

            return string.Join("", nodes.Select(n => n.Render(variables)));
        }
    }

    /// <summary>
    /// Node برای حلقه
    /// </summary>
    public class LoopNode : TemplateNode
    {
        /// <summary>
        /// حداکثر تعداد تکرار حلقه (برای جلوگیری از Infinite Loop)
        /// </summary>
        private const int MAX_LOOP_ITERATIONS = 100;

        public string CollectionName { get; set; }
        public List<TemplateNode> LoopContent { get; set; } = new List<TemplateNode>();

        public override string Render(Dictionary<string, object> variables)
        {
            if (variables == null || string.IsNullOrWhiteSpace(CollectionName))
            {
                return string.Empty;
            }

            // جستجوی collection در variables
            var key = variables.Keys.FirstOrDefault(k => 
                string.Equals(k, CollectionName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (key == null)
            {
                return string.Empty;
            }

            var collection = variables[key];

            // اگر collection یک IEnumerable است
            if (collection is System.Collections.IEnumerable enumerable && 
                !(collection is string))
            {
                var result = new System.Text.StringBuilder();
                int index = 0;
                int iterationCount = 0;

                foreach (var item in enumerable)
                {
                    // 🔒 Security: جلوگیری از Infinite Loop
                    iterationCount++;
                    if (iterationCount > MAX_LOOP_ITERATIONS)
                    {
                        throw new TemplateSecurityException(
                            $"حلقه {{#for {CollectionName}}} بیش از {MAX_LOOP_ITERATIONS} بار تکرار شده است. این ممکن است نشان‌دهنده Infinite Loop باشد.");
                    }

                    // ایجاد variables جدید برای هر item
                    var itemVariables = new Dictionary<string, object>(variables, StringComparer.OrdinalIgnoreCase);
                    
                    // اضافه کردن item properties به variables
                    if (item != null)
                    {
                        var itemType = item.GetType();
                        var properties = itemType.GetProperties();

                        foreach (var prop in properties)
                        {
                            try
                            {
                                var value = prop.GetValue(item);
                                itemVariables[prop.Name] = value;
                            }
                            catch
                            {
                                // Ignore errors
                            }
                        }

                        // اضافه کردن index
                        itemVariables["Index"] = index;
                        itemVariables["Count"] = index + 1;
                    }

                    // Render کردن محتوای حلقه
                    foreach (var node in LoopContent)
                    {
                        result.Append(node.Render(itemVariables));
                    }

                    index++;
                }

                return result.ToString();
            }

            return string.Empty;
        }
    }

    #endregion
}

