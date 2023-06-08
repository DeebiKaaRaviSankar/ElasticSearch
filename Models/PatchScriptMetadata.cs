using Microsoft.Extensions.Primitives;
using Nest;
using Newtonsoft.Json.Linq;
using QueryEditor.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using static Nest.JoinField;

namespace ElasticSearchPatchGenaration.Models
{
    public class PatchScriptMetadata
    {
        private const string DocSourcePath = "ctx._source";

        public string Script;
        public Dictionary<string, dynamic> Params;
        public int LastParameterIndex;

        public PatchScriptMetadata()
        {
        }

        public static PatchScriptMetadata GetPatchScriptMetadata(IEnumerable<FieldPatchDescriptor> descriptors)
        {
            var script = new StringBuilder();
            var parameters = new Dictionary<string, dynamic> { };
            int parameterIndex = 0;

            if (descriptors == null || descriptors.Any() == false)
            {
                return new PatchScriptMetadata();
            }

            descriptors.ToList().ForEach((descriptor) =>
            {
                var descriptorScriptMetaData = GetPatchScriptMetadata(descriptor, parameterIndex);

                script.Append(descriptorScriptMetaData.Script);

                descriptorScriptMetaData.Params.Keys.ToList().ForEach((key) =>
                {
                    if (parameters != null && parameters.ContainsKey(key))
                    {
                        parameters[key] = descriptorScriptMetaData.Params[key];
                    }
                    else
                    {
                        parameters.Add(key, descriptorScriptMetaData.Params[key]);
                    }

                    parameterIndex = descriptorScriptMetaData.LastParameterIndex;
                });
            });

            return new PatchScriptMetadata
            {
                Script = script.ToString(),
                Params = parameters,
                LastParameterIndex = parameterIndex,
            };
        }

        private class ScriptElement
        {
            public ScriptElement()
            {
            }

            public ScriptElement(StringBuilder headers, StringBuilder terminators)
            {
                this.Headers = headers.ToString();
                this.Terminators = terminators.ToString();
            }

            public ScriptElement(string headers, string terminators)
            {
                this.Headers = headers;
                this.Terminators = terminators;
            }

            public ScriptElement(string headers)
            {
                this.Headers = headers;
                this.Terminators = string.Empty;
            }

            public string Headers;
            public string Terminators;
        }

        private static PatchScriptMetadata GetPatchScriptMetadata(
            FieldPatchDescriptor descriptor,
            int parameterValueIndex)
        {
            var script = new StringBuilder();

            var parameters = new Dictionary<string, dynamic> { };
            var parameterValueKey = "item" + parameterValueIndex;

            var parentsPropertyMetadata = PropertyMetadata.GetParentsMetadata(
                descriptor.FieldPath);

            var fieldSelector = descriptor.FieldSelector;
            var paramsValue = $"params.{parameterValueKey}";

            var partsOfFieldPatchPath = descriptor.FieldPath.Split('.').ToList();

            var lastValidPatchTargetPath = GetLastValidPatchTargetPath(
                partsOfFieldPatchPath.Last(),
                parentsPropertyMetadata.ToList());

            var iterationIndex = 1;
            var iterationItemName = "item" + iterationIndex;
            var iterationItemIndex = "i" + iterationIndex;
            var previousIterionItemName = string.Empty;

            var fieldSelectorPath = fieldSelector != null 
                ? fieldSelector.FieldPath 
                : descriptor.FieldPath;

            var selectorElement = fieldSelectorPath.Split('.').Last();
            var fieldSelectorParentPath = fieldSelectorPath.Replace("." + selectorElement, string.Empty);

            var currentPath = partsOfFieldPatchPath.FirstOrDefault();

            PropertyMetadata parent = parentsPropertyMetadata.FirstOrDefault();
            var scriptElements = new List<ScriptElement> { };

            bool isPathTypeAppendOrRemove =
                                        descriptor.FieldPatchType == FieldPatchTypes.Remove ||
                                        descriptor.FieldPatchType == FieldPatchTypes.Append;

            switch (parent.Type)
            {
                case PropertyType.Nested:
                    {
                        if (partsOfFieldPatchPath.Count == 1)
                        {
                            var patchPath = $"{DocSourcePath}.{descriptor.FieldPath}";

                            scriptElements.Add(
                                new ScriptElement(
                                    GetNullCheckScript(
                                        patchPath,
                                        PropertyType.Nested)));

                            AddPatchValueModificationToScript(
                                descriptor,
                                patchPath,
                                ref parameterValueIndex,
                                script,
                                parameters,
                                ref parameterValueKey,
                                ref paramsValue,
                                scriptElements,
                                ref iterationIndex,
                                ref iterationItemName,
                                out iterationItemIndex,
                                out previousIterionItemName);

                            return new PatchScriptMetadata
                            {
                                Script = script.ToString(),
                                Params = parameters,
                                LastParameterIndex = parameterValueIndex,
                            };
                        }

                        AddSelectorScriptForRootProperty(
                            ref parameterValueIndex,
                            parent,
                            parameters,
                            ref parameterValueKey,
                            ref fieldSelector,
                            ref paramsValue,
                            ref iterationIndex,
                            ref iterationItemName,
                            out iterationItemIndex,
                            out previousIterionItemName,
                            out fieldSelectorPath,
                            ref selectorElement,
                            ref fieldSelectorParentPath,
                            scriptElements);
                        break;
                    }

                case PropertyType.Object:
                    {
                        var nextImmediateNonObjectProperty = GetNextImmediateNonObjectProperty(
                            parentsPropertyMetadata.ToList());

                        scriptElements.Add(
                            new ScriptElement(
                                GetNullCheckScriptForFirstSubsequentObjectPath(parentsPropertyMetadata.ToList())));

                        if (nextImmediateNonObjectProperty != null &&
                            nextImmediateNonObjectProperty.Path != descriptor.FieldPath &&
                            nextImmediateNonObjectProperty.Type == PropertyType.Nested &&
                            !string.IsNullOrEmpty(fieldSelectorParentPath))
                        {
                            selectorElement = isPathTypeAppendOrRemove && nextImmediateNonObjectProperty.Path != fieldSelectorParentPath
                                                ? string.Empty
                                                : selectorElement;
                            AddSelectorScriptForRootProperty(
                                    ref parameterValueIndex,
                                    parent,
                                    parameters,
                                    ref parameterValueKey,
                                    ref fieldSelector,
                                    ref paramsValue,
                                    ref iterationIndex,
                                    ref iterationItemName,
                                    out iterationItemIndex,
                                    out previousIterionItemName,
                                    out fieldSelectorPath,
                                    ref selectorElement,
                                    ref fieldSelectorParentPath,
                                    scriptElements);
                            break;
                        }
                        else if (nextImmediateNonObjectProperty != null && nextImmediateNonObjectProperty.Path == fieldSelectorParentPath
                                    && descriptor.FieldPatchType == FieldPatchTypes.Append)
                        {
                            AddSelectorScriptForRootProperty(
                                    ref parameterValueIndex,
                                    parent,
                                    parameters,
                                    ref parameterValueKey,
                                    ref fieldSelector,
                                    ref paramsValue,
                                    ref iterationIndex,
                                    ref iterationItemName,
                                    out iterationItemIndex,
                                    out previousIterionItemName,
                                    out fieldSelectorPath,
                                    ref selectorElement,
                                    ref fieldSelectorParentPath,
                                    scriptElements);
                            break;
                        }
                        else
                        {
                            AddPatchValueModificationToScript(
                                descriptor,
                                $"{DocSourcePath}.{descriptor.FieldPath}",
                                ref parameterValueIndex,
                                script,
                                parameters,
                                ref parameterValueKey,
                                ref paramsValue,
                                scriptElements,
                                ref iterationIndex,
                                ref iterationItemName,
                                out iterationItemIndex,
                                out previousIterionItemName);
                            return new PatchScriptMetadata
                            {
                                Script = script.ToString(),
                                Params = parameters,
                                LastParameterIndex = parameterValueIndex,
                            };
                        }
                    }

                default:
                    {
                        AddPatchValueModificationToScript(
                            descriptor,
                            $"{DocSourcePath}.{descriptor.FieldPath}",
                            ref parameterValueIndex,
                            script,
                            parameters,
                            ref parameterValueKey,
                            ref paramsValue,
                            scriptElements,
                            ref iterationIndex,
                            ref iterationItemName,
                            out iterationItemIndex,
                            out previousIterionItemName);

                        return new PatchScriptMetadata
                        {
                            Script = script.ToString(),
                            Params = parameters,
                            LastParameterIndex = parameterValueIndex,
                        };
                    }
            }

            var currentSelectorPath = fieldSelectorParentPath;
            if (string.IsNullOrEmpty(currentSelectorPath)
                || parent == null
                || parent.Type == PropertyType.Native)
            {
                var patchSelectorPath = string.IsNullOrEmpty(lastValidPatchTargetPath)
                    ? previousIterionItemName
                    : $"{previousIterionItemName}.{lastValidPatchTargetPath}";

                if (lastValidPatchTargetPath.Split('.').Length > 1)
                {
                    scriptElements.AddRange(
                                GetNullCheckScriptForPatchSelectorPath(
                                    patchSelectorPath,
                                    parentsPropertyMetadata.ToList()));
                }

                AddPatchValueModificationToScript(
                           descriptor,
                           patchSelectorPath,
                           ref parameterValueIndex,
                           script,
                           parameters,
                           ref parameterValueKey,
                           ref paramsValue,
                           scriptElements,
                           ref iterationIndex,
                           ref iterationItemName,
                           out iterationItemIndex,
                           out previousIterionItemName);
            }
            else
            {
                do
                {
                    fieldSelectorPath = fieldSelector == null ? string.Empty : fieldSelector.FieldPath;
                    selectorElement = fieldSelectorPath.Split('.').Last();
                    fieldSelectorParentPath = fieldSelectorPath.Replace("." + selectorElement, string.Empty);
                    currentSelectorPath = fieldSelectorParentPath != currentSelectorPath ? $"{currentSelectorPath}.{fieldSelectorParentPath}" : currentSelectorPath;

                    var lastSelector = fieldSelectorParentPath.Split('.').Last();
                    var fieldSelectorParentProperty = parentsPropertyMetadata.FirstOrDefault(_ => _.Path == currentSelectorPath);
                    var fieldSelectorParentPropertyType = fieldSelectorParentProperty != null
                        ? fieldSelectorParentProperty.Type
                        : PropertyType.Native;

                    if (fieldSelector.Child == null &&
                        fieldSelectorParentPropertyType == PropertyType.Nested &&
                        isPathTypeAppendOrRemove)
                    {
                        var iterator = $"{previousIterionItemName}.{lastSelector}";
                        
                        var patchSelectorPath = 
                            string.IsNullOrEmpty(lastValidPatchTargetPath) || lastValidPatchTargetPath == lastSelector
                                ? iterator
                                : $"{iterator}.{lastValidPatchTargetPath}";

                        scriptElements.Add(
                            new ScriptElement(
                                GetNullCheckScript(
                                    patchSelectorPath,
                                    fieldSelectorParentPropertyType)));

                        AddPatchValueModificationToScript(
                            descriptor,
                            patchSelectorPath,
                            ref parameterValueIndex,
                            script,
                            parameters,
                            ref parameterValueKey,
                            ref paramsValue,
                            scriptElements,
                            ref iterationIndex,
                            ref iterationItemName,
                            out iterationItemIndex,
                            out previousIterionItemName);
                        break;
                    }
                    else
                    {
                        AddInnerForLoopScript(
                            descriptor,
                            ref parameterValueIndex,
                            parameters,
                            ref parameterValueKey,
                            parentsPropertyMetadata,
                            fieldSelector,
                            ref paramsValue,
                            partsOfFieldPatchPath,
                            lastValidPatchTargetPath,
                            ref iterationIndex,
                            ref iterationItemName,
                            ref iterationItemIndex,
                            ref previousIterionItemName,
                            fieldSelectorPath,
                            fieldSelectorParentPath,
                            ref parent,
                            scriptElements,
                            lastSelector,
                            fieldSelectorParentPropertyType);

                        var iterator = $"{previousIterionItemName}.{lastSelector}[{iterationItemIndex}]";
                        
                        var patchSelectorPathSplitFromDescriptor = descriptor.FieldPath.Split(lastSelector + ".").Last();

                        var patchSelectorPath =
                            string.IsNullOrEmpty(lastValidPatchTargetPath) || lastValidPatchTargetPath == lastSelector
                                ? iterator
                                : $"{iterator}.{patchSelectorPathSplitFromDescriptor}";

                        AddPatchValueModificationToScript(
                             descriptor,
                             patchSelectorPath,
                             ref parameterValueIndex,
                             script,
                             parameters,
                             ref parameterValueKey,
                             ref paramsValue,
                             scriptElements,
                             ref iterationIndex,
                             ref iterationItemName,
                             out iterationItemIndex,
                             out previousIterionItemName);

                        fieldSelector = fieldSelector.Child;
                    }
                }
                while (fieldSelector != null);
            }

            return new PatchScriptMetadata
            {
                Script = script.ToString(),
                Params = parameters,
                LastParameterIndex = parameterValueIndex,
            };
        }

        private static void CombineScriptHeadersAndTerminators(
            StringBuilder script,
            List<ScriptElement> scriptElements)
        {
            scriptElements.ForEach((element) =>
            {
                script.Append(element.Headers);
            });

            scriptElements.ForEach((element) =>
            {
                script.Append(element.Terminators);
            });
        }

        private static void AddInnerForLoopScript(
            FieldPatchDescriptor descriptor,
            ref int parameterValueIndex,
            Dictionary<string, dynamic> parameters,
            ref string parameterValueKey,
            IEnumerable<PropertyMetadata> parentsPropertyMetadata,
            FieldSelector fieldSelector,
            ref string paramsValue,
            List<string> partsOfFieldPatchPath,
            string patchSelectorPathLastChild,
            ref int iterationIndex,
            ref string iterationItemName,
            ref string iterationItemIndex,
            ref string previousIterionItemName,
            string fieldSelectorPath,
            string fieldSelectorParentPath,
            ref PropertyMetadata parent,
            List<ScriptElement> scriptElements,
            string lastSelector,
            PropertyType fieldSelectorParentPropertyType)
        {
            var parentPath = parent.Path;
            switch (fieldSelectorParentPropertyType)
            {
                case PropertyType.Nested:
                    {
                        var itemToSelectOnIteration = $"{previousIterionItemName}.{fieldSelectorParentPath}[{iterationItemIndex}]";
                        var isDirectParent = parentsPropertyMetadata.Any(_ => _.Path == patchSelectorPathLastChild);
                        previousIterionItemName = string.IsNullOrEmpty(previousIterionItemName) ? iterationItemName : previousIterionItemName;

                        var patchSelector = descriptor.FieldPatchType == FieldPatchTypes.Append ?
                                $"{previousIterionItemName}.{patchSelectorPathLastChild}"
                                    : itemToSelectOnIteration
                                                    + (isDirectParent
                                                            ? string.Empty
                                                            : $".{patchSelectorPathLastChild}");

                        var fieldSelectorPathProperty = parentsPropertyMetadata.ToList().Find(_ => _.Path == fieldSelectorPath);
                        var fieldSelectorPathPropertyType = fieldSelectorPathProperty != null
                                                                ? fieldSelectorPathProperty.Type
                                                                : PropertyType.Native;

                        var patchPath = $"{previousIterionItemName}.{lastSelector}";
                        scriptElements.Add(new ScriptElement(GetNullCheckScript(patchPath, PropertyType.Nested), null));

                        scriptElements.Add(
                            GetForLoopScriptElement(
                                ref parameterValueIndex,
                                parameters,
                                ref parameterValueKey,
                                fieldSelector,
                                ref paramsValue,
                                iterationItemIndex,
                                patchPath));

                        parent = parentsPropertyMetadata.First(
                                    _ => _.Path == parentPath.Replace(
                                        '.' + parentPath.Split('.').Last(),
                                        string.Empty));

                        break;
                    }
                case PropertyType.Object:
                    {
                        var currentParent = parent;
                        var currentParentPath = currentParent.Path;
                        var pathPosition = partsOfFieldPatchPath.IndexOf(currentParentPath);

                        while (currentParent != null && currentParent.Type != PropertyType.Nested && currentParentPath != descriptor.FieldPath)
                        {
                            currentParentPath = currentParentPath + $".{partsOfFieldPatchPath[pathPosition + 1]}";
                            currentParent = parentsPropertyMetadata.First(_ => _.Path == currentParentPath);
                            pathPosition++;
                        };

                        scriptElements.Add(
                            new ScriptElement(
                                GetNullCheckScriptForFirstSubsequentObjectPath(parentsPropertyMetadata.ToList()), null));

                        scriptElements.Add(
                            GetForLoopScriptElement(
                                ref parameterValueIndex,
                                parameters,
                                ref parameterValueKey,
                                fieldSelector,
                                ref paramsValue,
                                iterationItemIndex,
                                $"{previousIterionItemName}.{fieldSelectorParentPath}"));

                        UpdateIteration(
                            ref iterationIndex,
                            ref iterationItemName,
                            out iterationItemIndex,
                            out previousIterionItemName);

                        parent = parentsPropertyMetadata.First(
                                    _ => _.Path == parentPath.Replace(
                                        '.' + parentPath.Split('.').Last(),
                                        string.Empty));

                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private static void AddPatchValueModificationToScript(
            FieldPatchDescriptor descriptor,
            string patchSelectorPath,
            ref int parameterValueIndex,
            StringBuilder script,
            Dictionary<string, dynamic> parameters,
            ref string parameterValueKey,
            ref string paramsValue,
            List<ScriptElement> scriptElements,
            ref int iterationIndex,
            ref string iterationItemName,
            out string iterationItemIndex,
            out string previousIterionItemName)
        {
            scriptElements.Add(GetPatchValueModificationScriptByPatchType(
                                    descriptor,
                                    patchSelectorPath,
                                    paramsValue));

            if(descriptor.PatchValue != null)
            {
                paramsValue = AddPatchValueParam(
                        descriptor,
                        ref parameterValueIndex,
                        parameters,
                        ref parameterValueKey);
            }

            UpdateIteration(ref iterationIndex,
                            ref iterationItemName,
                            out iterationItemIndex,
                            out previousIterionItemName);

            CombineScriptHeadersAndTerminators(script, scriptElements);
        }

        private static PropertyMetadata GetNextImmediateNonObjectProperty(
           List<PropertyMetadata> parentsPropertyMetadata)
        {
            return parentsPropertyMetadata.ToList()
                .FirstOrDefault(parent => parent.Type != PropertyType.Object);
        }

        private static string GetLastValidPatchTargetPath(
           string lastSelectorPath,
           List<PropertyMetadata> parentsPropertyMetadata)
        {
            var result = parentsPropertyMetadata.ToList()
                .LastOrDefault(parent => parent.Type != PropertyType.Nested);

            var indexOfResult = parentsPropertyMetadata.IndexOf(result);
            var indexOfLastNestedPath = indexOfResult - 1;

            if (indexOfResult < 1 || indexOfLastNestedPath < 1 || result == null)
            {
                return lastSelectorPath;
            }

            var lastProperty = parentsPropertyMetadata
                .Last()
                .Path
                .Replace(
                    parentsPropertyMetadata[indexOfLastNestedPath].Path + ".",
                    string.Empty);

            return lastProperty;
        }

        private static PropertyMetadata GetLastValidPatchTargetPropertyMetadata(
            List<PropertyMetadata> parentsPropertyMetadata)
        {
            var result = parentsPropertyMetadata.ToList()
               .LastOrDefault(parent => parent.Type != PropertyType.Nested);

            var indexOfResult = parentsPropertyMetadata.IndexOf(result);
            var indexOfLastNestedPath = indexOfResult - 1;

            if (indexOfResult < 1 || indexOfLastNestedPath < 1 || result == null)
            {
                return null;
            }

            return parentsPropertyMetadata.Last();
        }

        private static IEnumerable<ScriptElement> GetNullCheckScriptForPatchSelectorPath(
            string path,
            List<PropertyMetadata> parentsPropertyMetadata)
        {
            var parts = path.Split('.').ToList();

            var result = new List<ScriptElement>();

            parts.ForEach(
                (part) => {
                    var propertyTypeOfCurrentPath = parentsPropertyMetadata
                        .LastOrDefault(parent => part == parent.Path.Split('.').Last());

                    if (propertyTypeOfCurrentPath == null)
                    {
                        return;
                    }

                    var index = path.IndexOf(part);
                    var currentPath = path.Substring(0, index + part.Length);

                    result.Add(
                        new ScriptElement(
                            GetNullCheckScript(
                                currentPath,
                                propertyTypeOfCurrentPath.Type)));
                });

            return result;
        }

        private static string GetNullCheckScriptForFirstSubsequentObjectPath(
            List<PropertyMetadata> parentsPropertyMetadata)
        {
            var script = new StringBuilder();
            var currentParentPath = string.Empty;

            foreach (var parent in parentsPropertyMetadata)
            {
                if (parent.Type != PropertyType.Object)
                {
                    break;
                }

                script.Append(
                        PatchScriptMetadata.GetNullCheckScript(
                            $"{DocSourcePath}.{parent.Path}",
                            PropertyType.Object));
            }

            return script.ToString();
        }

        private static void AddSelectorScriptForRootProperty(
            ref int parameterValueIndex,
            PropertyMetadata parent,
            Dictionary<string, dynamic> parameters,
            ref string parameterValueKey,
            ref FieldSelector fieldSelector,
            ref string paramsValue,
            ref int iterationIndex,
            ref string iterationItemName,
            out string iterationItemIndex,
            out string previousIterionItemName,
            out string fieldSelectorPath,
            ref string selectorElement,
            ref string fieldSelectorParentPath,
            List<ScriptElement> scriptElements)
        {
            scriptElements.Add(
                GetForLoopHeadersAndTerminators(
                        ref parameterValueIndex,
                        parameters,
                        parent,
                        ref parameterValueKey,
                        fieldSelector,
                        ref paramsValue,
                        iterationItemName,
                        selectorElement,
                        fieldSelectorParentPath));

            UpdateIteration(
                ref iterationIndex,
                ref iterationItemName,
                out iterationItemIndex,
                out previousIterionItemName);

            fieldSelector = UpdateFieldSelector(
                fieldSelector,
                out fieldSelectorPath,
                out selectorElement,
                out fieldSelectorParentPath);
        }

        private static FieldSelector UpdateFieldSelector(
            FieldSelector fieldSelector,
            out string fieldSelectorPath,
            out string selectorElement,
            out string fieldSelectorParentPath)
        {
            fieldSelector = fieldSelector == null ? null : fieldSelector.Child;
            fieldSelectorPath = fieldSelector != null ? fieldSelector.FieldPath : string.Empty;
            selectorElement = fieldSelectorPath.Split('.').Last();
            fieldSelectorParentPath = fieldSelectorPath.Replace("." + selectorElement, string.Empty);
            return fieldSelector;
        }

        private static void UpdateIteration(
            ref int iterationIndex,
            ref string iterationItemName,
            out string iterationItemIndex,
            out string previousIterionItemName)
        {
            previousIterionItemName = iterationItemName;

            iterationItemIndex = "i" + iterationIndex;
            iterationIndex++;
            iterationItemName = "item" + iterationIndex;
        }

        private static ScriptElement GetForLoopHeadersAndTerminators(
            ref int parameterValueIndex,
            Dictionary<string, dynamic> parameters,
            PropertyMetadata parent,
            ref string parameterValueKey,
            FieldSelector fieldSelector,
            ref string paramsValue,
            string iterationItemName,
            string selectorPropertyName,
            string nestedPath)
        {
            var headers = new StringBuilder();
            var terminators = new StringBuilder();

            if(parent.Type.Equals("Nested"))
            {
                headers.Append(
                $"{PatchScriptMetadata.GetNullCheckScript($"{DocSourcePath}.{parent.Path}", PropertyType.Nested)}" +
                $"for({iterationItemName} in {DocSourcePath}.{parent.Path})" +
                "{");
                terminators.Append("}");
            }

            else
            {
                headers.Append(
                $"{PatchScriptMetadata.GetNullCheckScript($"{DocSourcePath}.{nestedPath}", PropertyType.Nested)}" +
                $"for({iterationItemName} in {DocSourcePath}.{nestedPath})" +
                "{");
                terminators.Append("}");
            }

            if (!parent.Equals(nestedPath))
            {
                if (!string.IsNullOrEmpty(selectorPropertyName))
                {
                    headers.Append(
                        $"if({iterationItemName}.{selectorPropertyName} == {paramsValue})" +
                        "{");
                    terminators.Append("}");

                    paramsValue = AddSelectorParam(
                                ref parameterValueIndex,
                                parameters,
                                ref parameterValueKey,
                                fieldSelector);
                }
            }


            return new ScriptElement(headers, terminators);
        }

        private static ScriptElement GetForLoopScriptElement(
            ref int parameterValueIndex,
            Dictionary<string, dynamic> parameters,
            ref string parameterValueKey,
            FieldSelector fieldSelector,
            ref string paramsValue,
            string iterationItemName,
            string itemToIterateThrough)
        {
            var headers = new StringBuilder();
            var terminators = new StringBuilder();

            headers.Append(
                $"for(int {iterationItemName} = 0; {iterationItemName} < {itemToIterateThrough}; {iterationItemName}++;)" +
                "{");

            terminators.Append("}");

            if (fieldSelector != null)
            {
                var selectorPath = fieldSelector.FieldPath.Split('.').Last();
                headers.Append(
                    $"if({itemToIterateThrough}[{iterationItemName}].{selectorPath} == {paramsValue})" +
                    "{"
                    );
                terminators.Append("}");
            }

            paramsValue = AddSelectorParam(
                            ref parameterValueIndex,
                            parameters,
                            ref parameterValueKey,
                            fieldSelector);

            return new ScriptElement(headers, terminators);
        }

        private static string AddSelectorParam(
            ref int parameterValueIndex,
            Dictionary<string, dynamic> parameters,
            ref string parameterValueKey,
            FieldSelector fieldSelector)
        {
            string paramsValue;
            if (fieldSelector != null)
            {
                parameters.Add(parameterValueKey, fieldSelector.Selector);
            }
            /*parameterValueIndex++;*/
            parameterValueKey = "item" + parameterValueIndex;
            paramsValue = $"params.{parameterValueKey}";
            return paramsValue;
        }

        private static string AddPatchValueParam(
            FieldPatchDescriptor descriptor,
            ref int parameterValueIndex,
            Dictionary<string, dynamic> parameters,
            ref string parameterValueKey)
        {
            string paramsValue;
            parameters.Add(parameterValueKey, descriptor.PatchValue);
            parameterValueIndex++;
            parameterValueKey = "item" + parameterValueIndex;
            paramsValue = $"params.{parameterValueKey}";
            return paramsValue;
        }

        private static object GetLastSelectorValue(FieldPatchDescriptor descriptor)
        {
            var selector = descriptor.FieldSelector;
            var value = selector == null ? null : selector.Selector;
            while (selector != null)
            {
                if (selector.Child == null)
                {
                    value = selector.Selector;
                }
                selector = selector != null ? selector.Child : null;
            }
            return value;
        }

        private static ScriptElement GetPatchValueModificationScriptByPatchType(
            FieldPatchDescriptor descriptor,
            string patchSelectorPath,
            dynamic patchValueParamKey,
            bool isReplaceInList = false,
            FieldSelector fieldSelector = null)
        {
            var headers = new StringBuilder();
            var isPatchValueAnEnumerable = descriptor.PatchValue is IEnumerable;
            var fieldPathType = descriptor.FieldPatchType;

            switch (fieldPathType)
            {
                case FieldPatchTypes.Increment:
                    {
                        headers.Append($"{patchSelectorPath} ? {patchSelectorPath} += {patchValueParamKey} : {patchValueParamKey};");
                        break;
                    }

                case FieldPatchTypes.Decrement:
                    {
                        headers.Append($"{patchSelectorPath} ? {patchSelectorPath} -= {patchValueParamKey} : {patchValueParamKey};");
                        break;
                    }

                case FieldPatchTypes.ReplaceExistingValues:
                    {
                        if (isReplaceInList)
                        {
                            headers.Append(
                                   $"{GetNullCheckScript($"{patchSelectorPath}", PropertyType.Nested)}" +
                                   $"if({patchSelectorPath} instanceof List)" +
                                   "{" +
                                        $"for(int i = 0;i < {patchSelectorPath}.length; i++)" +
                                        "{" +
                                            $"if ({patchSelectorPath}[i].id == {fieldSelector.Selector})" +
                                            "{" +
                                                $"{patchSelectorPath}[i] = {patchValueParamKey};" +
                                            "}" +
                                        "}" +
                                  "}");
                            break;
                        }

                        headers.Append($"{patchSelectorPath} = {patchValueParamKey};");
                        break;
                    }

                case FieldPatchTypes.Remove:
                    {
                        headers.Append(
                                   $"if({patchSelectorPath} instanceof List)" +
                                   "{" +
                                        $"{patchSelectorPath}." +
                                                (isPatchValueAnEnumerable
                                                    ? $"removeAll({patchValueParamKey})"
                                                    : $"removeIf(item -> item.id.equals({patchValueParamKey}));") +
                                   "}" +
                                   $"else" +
                                   "{" +
                                        $"{patchSelectorPath} = new ArrayList();" +
                                   "}");

                        break;
                    }

               /* case FieldPatchTypes.Remove:
                    {
                        headers.Append(
                                   $"if({patchSelectorPath} instanceof List)" +
                                   "{" +
                                        $"{patchSelectorPath}." +
                                                (isPatchValueAnEnumerable
                                                    ? $"removeAll({patchValueParamKey})"
                                                    : $"removeIf(item -> item.id.equals({patchValueParamKey}));") +
                                   "}" +
                                   $"else" +
                                   "{" +
                                        $"{patchSelectorPath} = new ArrayList();" +
                                   "}");

                        break;
                    }*/

                case FieldPatchTypes.Append:
                    {
                        headers.Append(
                            $"{GetNullCheckScript($"{patchSelectorPath}", PropertyType.Nested)}" +
                            $"if ({patchSelectorPath} instanceof List)" +
                            "{"
                                + GetAppendScript(descriptor, patchSelectorPath, patchValueParamKey) +
                            "}" +
                            "else" +
                            "{" +
                                (isPatchValueAnEnumerable
                                        ? $"{patchSelectorPath} = {patchValueParamKey}"
                                        : $"{patchSelectorPath} = new ArrayList();" +
                                          $"{patchSelectorPath}.add({patchValueParamKey});") +
                            "}");
                        break;
                    }

                default:
                    {
                        break;
                    }
            }

            return new ScriptElement(headers.ToString());
        }

        private static string GetAppendScript(
            FieldPatchDescriptor descriptor,
            string patchSelectorPath,
            dynamic patchValueParamKey)
        {
            var isPatchValueAnEnumerable = descriptor.PatchValue is IEnumerable;

            if (isPatchValueAnEnumerable)
            {
                return
                    $"for(int k=0; k<{patchValueParamKey}.length; k++)" +
                    "{" +
                         GetAppendScript(
                             patchSelectorPath,
                             patchValueParamKey,
                             $"{patchValueParamKey}[k]",
                             isPatchValueAnEnumerable) +
                    "}";
            }

            return GetAppendScript(
                patchSelectorPath,
                patchValueParamKey,
                patchValueParamKey,
                isPatchValueAnEnumerable);
        }

        private static string GetAppendScript(
            string patchSelectorPath,
            dynamic patchValueParamKey,
            string itemToAddKey,
            bool isPatchValueAnEnumerable)
        {
            return
                $"if({itemToAddKey}.id != null)" +
                "{" +
                        $"List list = {patchSelectorPath};" +
                        $"if(list.stream().anyMatch(existingItem -> existingItem.id.equals({itemToAddKey}.id)))" +
                        "{" +
                            $"{patchSelectorPath}.removeIf(item -> item.id.equals({itemToAddKey}.id));"
                            + $"{patchSelectorPath}.add({itemToAddKey});" +
                        "}" +
                        "else" +
                        "{" +
                            $"{patchSelectorPath}.add({itemToAddKey});" +
                        "}" +
                "}" +
                "else" +
                "{" +
                         $"{patchSelectorPath}." +
                                    (isPatchValueAnEnumerable
                                        ? $"addAll({patchValueParamKey})"
                                        : $"add({itemToAddKey})") +
                "}";
        }

        private static string GetNullCheckScript(string pathToCheck, PropertyType propertyType)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"if({pathToCheck} == null) {{");

            switch (propertyType)
            {
                case PropertyType.Nested:
                    stringBuilder.Append($"{pathToCheck} = new ArrayList();");
                    break;
                case PropertyType.Object:
                    stringBuilder.Append($"{pathToCheck} = new HashMap();");
                    break;
                default:
                    return string.Empty;
            }

            stringBuilder.Append('}');

            return stringBuilder.ToString();
        }
    }
}
