using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace StringWriterBench
{
	public static class Lipsum
	{
		private const String _lipsum = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque nisi enim, ultricies sed sollicitudin ut, vestibulum vitae dui. Mauris porta vitae purus sed tristique. Aliquam et leo auctor, dignissim nulla et, porttitor leo. Integer bibendum lobortis sapien ut suscipit. Integer malesuada velit nec metus consequat, luctus pellentesque sapien fermentum. Phasellus libero risus, convallis non condimentum non, mollis vitae erat. Suspendisse pellentesque aliquam eleifend. Donec sodales suscipit orci, et malesuada urna aliquet non.";

		public static void AppendLipsumSubstring( StringBuilder sb, Random rng )
		{
			Int32 lipsumStart  = rng.Next( maxValue: _lipsum.Length );

			Int32 lipsumMaxLength = Math.Min( 20, _lipsum.Length - lipsumStart );

			Int32 lipsumLength = rng.Next( maxValue: lipsumMaxLength );

			_ = sb.Append( _lipsum, startIndex: lipsumStart, count: lipsumLength );
		}

		public static String GetLipsumSubstring( Random rng )
		{
			Int32 lipsumStart  = rng.Next( maxValue: _lipsum.Length );

			Int32 lipsumMaxLength = Math.Min( 20, _lipsum.Length - lipsumStart );

			Int32 lipsumLength = rng.Next( maxValue: lipsumMaxLength );

			return _lipsum.Substring( startIndex: lipsumStart, length: lipsumLength );
		}
	}

	public static class Arrange
	{
		private static readonly String[] _i32Fmts = new[] { "N0", "N2", "d", "C2", "E6", "X8" };

		private static readonly IReadOnlyList<( String format, Object[] args )> _runs = CreateRuns();

		public static List<(String format, Object[] args)> CreateRuns()
		{
			Random rng = new Random( Seed: 1337 );

			StringBuilder reusableSB = new StringBuilder( capacity: 1024 );

			return Enumerable
				.Range( start: 0, count: 1024 )
				.Select( n => (
					format: CreateCompositeFormatString    ( placeholderCount: n, rngSeed: rng.Next(), reusableSB ),
					args  : CreateCompositeFormatStringArgs( length: n, rngSeed: rng.Next() )
				) )
				.ToList();
		}

		private static String CreateCompositeFormatString( Int32 placeholderCount, Int32 rngSeed, StringBuilder sb )
		{
			try
			{
				Random rng = new Random( Seed: rngSeed );

				for( Int32 i = 0; i < placeholderCount; i++ )
				{
					// Append random text:
					Lipsum.AppendLipsumSubstring( sb, rng );

					_ = sb.Append( ' ' );

					AppendCompositePlaceholder(
						sb   : sb,
						index: i,
						align: ( rng.NextDouble() >= 0.50 ) ? ( rng.Next( maxValue: 21 ) ) : (Int32?)null, // 50% chance of rendering composite format alignment
						fmt  : ( rng.NextDouble() >= 0.25 ) ? ( GetRandomElement( _i32Fmts, rng ) ) : null  // 75% chance of rendering composite format args
					);

					_ = sb.Append( ' ' );
				}

				return sb.ToString();
			}
			finally
			{
				if( sb.Length > 10240 )
				{
					sb.Length = 0;
					sb.Capacity = 10240;
				}
				else
				{
					sb.Length = 0;
				}
			}
		}

		private static void AppendCompositePlaceholder( StringBuilder sb, Int32 index, Int32? align, String fmt )
		{
			_ = sb.Append( '{' );
			_ = sb.Append( index.ToString( CultureInfo.InvariantCulture ) );

			if( align.HasValue )
			{
				_ = sb.Append( ',' );
				_ = sb.Append( align.Value.ToString( CultureInfo.InvariantCulture ) );
			}

			if( fmt != null )
			{
				_ = sb.Append( ':' );
				_ = sb.Append( fmt );
			}

			_ = sb.Append( '}' );
		}

		private static T GetRandomElement<T>( IReadOnlyList<T> items, Random rng )
		{
			Int32 idx = rng.Next( maxValue: items.Count );
			return items[idx];
		}

		private static Object[] CreateCompositeFormatStringArgs( Int32 length, Int32 rngSeed )
		{
			if( length == 0 ) return Array.Empty<Object>();

			Random rng = new Random( Seed: rngSeed );

			// Use the rngSeed for `compositeFormatArgs[0]` as proof of consistent rng values.

			Object[] compositeFormatArgs = new Object[ length ];
			compositeFormatArgs[0] = rngSeed;
			for( Int32 i = 1; i < length; i++ )
			{
				Double rand = rng.NextDouble();
				if( rand > 0.9d ) // 10% chance
				{
					compositeFormatArgs[i] = null;
				}
				else if( rand > 0.4d ) // 50% chance of String
				{
					compositeFormatArgs[i] = Lipsum.GetLipsumSubstring( rng );
				}
				else // 40% chance of Int32
				{
					compositeFormatArgs[i] = rng.Next();
				}
			}

			return compositeFormatArgs;
		}

	}
}
