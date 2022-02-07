using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			if( args.Length >= 1 && args[0] == "hmm" )
			{
				Program p = new Program();

				{
					Stopwatch setupSW = Stopwatch.StartNew();
					p.Setup();
					Console.WriteLine( "Took {0:N0}ms to create composite format data.", setupSW.ElapsedMilliseconds );

					// Dump the rngSeeds rendered at the beginning of the first and last strings, for confidence the data hasn't changed.
					String firstString = _runs[0].format;
					String lastString  = _runs[ _runs.Count - 1 ].format;

					Console.WriteLine( "_runs[0]: {0}", firstString.Substring( startIndex: 0, length: firstString.IndexOf( ')' ) + 1 ) );
					Console.WriteLine( "_runs[n]: {0}", lastString .Substring( startIndex: 0, length: lastString .IndexOf( ')' ) + 1 ) );
					Console.WriteLine();

					/*
					In both .NET Fx 4.8 and .NET Core 3.1, I get:

					_runs[0]: (n:     0, rngSeed:  448584296 )
					_runs[n]: (n: 1,023, rngSeed: 1218829794 )
					*/
				}

				Console.WriteLine( "Warm-up run..." );
				p.RunAll(); // Warm-up

				Console.WriteLine( "Second run..." );
				p.RunAll(); // For real

				Console.WriteLine( "Done" );
			}
			else
			{
				Summary[] summary = BenchmarkRunner.Run( typeof(Program).Assembly );

				Console.WriteLine( "Done" );
			}
		}

		private void RunAll()
		{
			Stopwatch sw = Stopwatch.StartNew();

			this.UseStringBuilder();

			TimeSpan timeSB = sw.Elapsed;
			sw.Restart();

			this.UseStringWriter();

			TimeSpan timeSW = sw.Elapsed;
			sw.Restart();

			this.UseMyStringWriter();

			TimeSpan timeMySW = sw.Elapsed;

			//

			Console.WriteLine( "StringBuilder : {0:N0}ms", timeSB  .TotalMilliseconds );
			Console.WriteLine( "StringWriter  : {0:N0}ms", timeSW  .TotalMilliseconds );
			Console.WriteLine( "MyStringWriter: {0:N0}ms", timeMySW.TotalMilliseconds );
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

		[Benchmark]
		public void UseMyStringWriter()
		{
			if( _runs is null ) throw new InvalidOperationException( "_runs is null." );

			StringBuilder sb = new StringBuilder();

			using( MyStringWriter writer = new MyStringWriter( sb, CultureInfo.InvariantCulture ) )
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

	public class MyStringWriter : StringWriter
	{
		public MyStringWriter( StringBuilder sb, IFormatProvider formatProvider )
			: base( sb, formatProvider )
		{
			this.sb = sb;
			this.formatProvider = formatProvider;
		}

		private readonly StringBuilder sb;
		private readonly IFormatProvider formatProvider;

		public override void Write( String format, params Object[] arg )
		{
			_ = this.sb.AppendFormat( this.formatProvider, format: format, args: arg );
		}

		public override void Write( String format, Object arg0, Object arg1, Object arg2 )
		{
			_ = this.sb.AppendFormat( this.formatProvider, format: format, arg0: arg0, arg1: arg1, arg2: arg2 );
		}

		public override void Write( String format, Object arg0, Object arg1 )
		{
			_ = this.sb.AppendFormat( this.formatProvider, format: format, arg0: arg0, arg1: arg1 );
		}

		public override void Write( String format, Object arg0 )
		{
			_ = this.sb.AppendFormat( this.formatProvider, format: format, arg0: arg0 );
		}

		//

		public override void WriteLine( String format, params Object[] arg )
		{
			_ = this.sb.AppendFormat( this.formatProvider, format: format, args: arg );
			_ = this.sb.AppendLine();
		}

		public override void WriteLine( String format, Object arg0, Object arg1, Object arg2 )
		{
			_ = this.sb.AppendFormat( this.formatProvider, format: format, arg0: arg0, arg1: arg1, arg2: arg2 );
			_ = this.sb.AppendLine();
		}

		public override void WriteLine( String format, Object arg0, Object arg1 )
		{
			_ = this.sb.AppendFormat( this.formatProvider, format: format, arg0: arg0, arg1: arg1 );
			_ = this.sb.AppendLine();
		}

		public override void WriteLine( String format, Object arg0 )
		{
			_ = this.sb.AppendFormat( this.formatProvider, format: format, arg0: arg0 );
			_ = this.sb.AppendLine();
		}
	}
}
