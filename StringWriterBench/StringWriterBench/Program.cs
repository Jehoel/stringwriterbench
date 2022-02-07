using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace StringWriterBench
{
	[Config(typeof(MyBenchConfig))]
	public class Program
	{
		public static void Main( String[] args )
		{
			Summary[] summary = BenchmarkRunner.Run( typeof(Program).Assembly );

			Console.WriteLine( "Done" );
		}

		[GlobalSetup]
		public void Setup()
		{
			if( _runs is null )
			{
				_runs = Arrange.CreateRuns();
			}
		}

		private static List<(String format, Object[] args)> _runs;

		[Benchmark]
		public void UseStringBuilder()
		{
			if( _runs is null ) throw new InvalidOperationException( "_runs is null." );

			StringBuilder sb = new StringBuilder();

			foreach( (String format, Object[] args) in _runs )
			{
				_ = sb.AppendFormat( CultureInfo.InvariantCulture, format: format, args: args );
				_ = sb.AppendLine();
			}
		}

		[Benchmark]
		public void UseStringWriter()
		{
			if( _runs is null ) throw new InvalidOperationException( "_runs is null." );

			StringBuilder sb = new StringBuilder();

			using( StringWriter writer = new StringWriter( sb, CultureInfo.InvariantCulture ) )
			{
				foreach( (String format, Object[] args) in _runs )
				{
					writer.Write( format: format, arg: args );
					writer.WriteLine();
				}
			}
		}
	}

	public class MyBenchConfig : ManualConfig
	{
		public MyBenchConfig()
		{
			_ = this.AddDiagnoser( MemoryDiagnoser.Default );
		}
	}
}
