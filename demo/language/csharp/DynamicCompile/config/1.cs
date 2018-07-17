//[head]
//namespace:DyanmicCodeGen
//class:Test1
//method:Print:s
//[end]

using System;

namespace DyanmicCodeGen
{
    class Test1
    {
	private static int Plus(int a, int b)
	{
        Console.WriteLine("Test1 static fun(Plus): done!");
		return 3*(a+b);
	}
    public static void Print()
    {
        Console.WriteLine("Test1 static fun(Print): done!");

        Program.print();

        int a=2;
        int b=3;
  	    int c = Program.Plus(a,b);
        Console.WriteLine("plus result:"+c);
        int d = Plus(a,b);
        Console.WriteLine("plus result:"+d);
    }
    }
}