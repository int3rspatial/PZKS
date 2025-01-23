using Python.Runtime;

namespace Lab3
{
    public class PythonRuntimeControler
    {
        public PythonRuntimeControler()
        {
            Runtime.PythonDLL = @"C:\Users\vlad_root\AppData\Local\Programs\Python\Python312\python312.dll";
            PythonEngine.Initialize();
        }

        public string ExpandExpression(string checkedExpression)
        {
            string expandedExpression = "";

            using (Py.GIL())
            {
                var pythonScript = Py.Import(@"Lab2\expand.py");
                var expr = new PyString(checkedExpression);
                var result = pythonScript.InvokeMethod("expand_expr", new PyObject[] { expr }).ToString();
                expandedExpression = result ?? "";
            }

            return expandedExpression;
        }

        public string SimplifyExpression(string expression)
        {
            return OtherFunctions(invokeMethod: "simplify_expr", expression);
        }

        public string SeparateVarsInExpression(string expression)
        {
            return OtherFunctions(invokeMethod: "separatevars_in_expr", expression);
        }

        public string CondenseExpression(string expression)
        {
            return OtherFunctions(invokeMethod: "together_expr", expression);
        }

        private string OtherFunctions(string invokeMethod, string expression)
        {
            string expandedExpression = "";
            using (Py.GIL())
            {
                var pythonScript = Py.Import(@"Lab3\lab3_scripts.py");
                var expr = new PyString(expression);
                var result = pythonScript.InvokeMethod(invokeMethod, new PyObject[] { expr }).ToString();
                expandedExpression = result ?? "";
            }
            return expandedExpression;
        }

        public string CollectVarsInExpression(string expression, string variable)
        {
            return FunctionsThatNeedVariable(invokeMethod: "collect_vars", expression, variable);
        }

        public string ApartVarInExpression(string expression, string variable)
        {
            return FunctionsThatNeedVariable(invokeMethod: "apart_var", expression, variable);
        }

        public string ApartSepvarsTogether(string expression, string variable)
        {
            return FunctionsThatNeedVariable(invokeMethod: "apart_sepvars_together", expression, variable);
        }

        private string FunctionsThatNeedVariable(string invokeMethod, string expression, string variable)
        {
            string expandedExpression = "";
            using (Py.GIL())
            {
                var pythonScript = Py.Import("lab3_scripts");
                var expr = new PyString(expression);
                var var_expr = new PyString(variable);
                var result = pythonScript.InvokeMethod(invokeMethod, new PyObject[] { expr, var_expr }).ToString();
                expandedExpression = result ?? "";
            }
            return expandedExpression;
        }

        public void Shutdown()
        {
            //temporary solution until they fix binaryformatter issue
            AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
            PythonEngine.Shutdown();
            AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", false);
        }
    }
}
