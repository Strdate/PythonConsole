namespace PythonConsole
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public static class ToolControllerUtil
    {
        public static void AddExtraToolToController<T>(this ToolController toolController)
            where T : ToolBase
        {
            if (toolController == null) {
                throw new ArgumentException("Tool controller not found!");
            }

            var tool = toolController.gameObject.AddComponent<T>();
            var fieldInfo = typeof(ToolController).GetField("m_tools", BindingFlags.Instance | BindingFlags.NonPublic);
            var tools = (ToolBase[])fieldInfo.GetValue(toolController);
            var initialLength = tools.Length;
            Array.Resize(ref tools, initialLength + 1);
            var dictionary =
                (Dictionary<Type, ToolBase>)typeof(ToolsModifierControl).GetField("m_Tools", BindingFlags.Static | BindingFlags.NonPublic)
                    .GetValue(null);
            dictionary.Add(tool.GetType(), tool);
            tools[initialLength] = tool;

            fieldInfo.SetValue(toolController, tools);
        }
    }
}