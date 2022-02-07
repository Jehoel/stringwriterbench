using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace StringWriterBench
{
	public static class Arrange
	{
		private const String _lipsum = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque nisi enim, ultricies sed sollicitudin ut, vestibulum vitae dui. Mauris porta vitae purus sed tristique. Aliquam et leo auctor, dignissim nulla et, porttitor leo. Integer bibendum lobortis sapien ut suscipit. Integer malesuada velit nec metus consequat, luctus pellentesque sapien fermentum. Phasellus libero risus, convallis non condimentum non, mollis vitae erat. Suspendisse pellentesque aliquam eleifend. Donec sodales suscipit orci, et malesuada urna aliquet non.";

		private static readonly String[] _i32Fmts = new[] { "N0", "N2", "d", "C2", "E6", "X8" };

		private static readonly IReadOnlyList<( String format, Object[] args )> _runs = CreateRuns();

		public static List<(String format, Object[] args)> CreateRuns()
		{
			Random rng = new Random( Seed: 1337 );

			StringBuilder reusableSB = new StringBuilder( capacity: 1024 );

			return Enumerable
				.Range( start: 0, count: 1024 )
				.Select( n => (
					format: CreateCompositeFormatString( placeholderCount: n, rngSeed: rng.Next(), reusableSB ),
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
					{
						Int32 lipsumStart  = rng.Next( maxValue: _lipsum.Length );
						Int32 lipsumLength = rng.Next( maxValue: Math.Min( 10, _lipsum.Length - lipsumStart ) ); // lipsumLength is random number between 0-10, *or* 0-(remaining lipsum chars)

						_ = sb.Append( _lipsum, startIndex: lipsumStart, count: lipsumLength );
					}

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

			// For now, use only Int32 values. Use the rngSeed as initial value for proof of consistent rng values:
			Object[] arr = new Object[ length ];
			arr[0] = rngSeed;
			for( Int32 i = 1; i < length; i++ )
			{
				arr[i] = rng.Next();

				// ...with some nulls:
				if( rng.NextDouble() > 0.9f ) // 10% chance
				{
					arr[i] = null;
				}
			}

			return arr;
		}

	}
}
