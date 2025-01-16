import sympy

def apart_var(expression, variable):
	return sympy.apart(expression, x=sympy.Symbol(variable))

def simplify_expr(expression):
	return sympy.simplify(expression)

def separatevars_in_expr(expression):
	return sympy.separatevars(expression)

def together_expr(expression):
	return sympy.together(expression)

def apart_sepvars_together(expression, variable):
	return sympy.apart(sympy.separatevars(sympy.together(expression)), x=sympy.Symbol(variable))

def collect_vars(expression, variable):
	return sympy.collect(expression, sympy.Symbol(variable))