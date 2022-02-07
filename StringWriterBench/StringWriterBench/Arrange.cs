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
				.Select( n => ( n, rngSeed: rng.Next() ) )
				.Select( t => ( t.n, t.rngSeed, types: GetRunFormatArgTypes( n: t.n, valueCount: t.n, t.rngSeed ).ToList() ) )
				.Select( t => (

					format: CreateCompositeFormatString    ( t.n, valueCount: t.n, rngSeed: t.rngSeed, t.types, reusableSB ),
					args  : CreateCompositeFormatStringArgs( t.n, valueCount: t.n, rngSeed: t.rngSeed, t.types )
				) )
				.ToList();
		}

		private static IEnumerable<Type> GetRunFormatArgTypes( Int32 n, Int32 valueCount, Int32 rngSeed )
		{
			Random rng = new Random( Seed: rngSeed );

			for( Int32 i = 0; i < valueCount; i++ )
			{
				Double rand = rng.NextDouble();
				if( rand > 0.9d ) // 10% chance
				{
					yield return typeof(void);
				}
				else if( rand > 0.4d ) // 50% chance of String
				{
					yield return typeof(String);
				}
				else // 40% chance of Int32
				{
					yield return typeof(Int32);
				}
			}
		}

		private static String CreateCompositeFormatString( Int32 n, Int32 valueCount, Int32 rngSeed, IReadOnlyList<Type> types, StringBuilder /*reusableSB*/ sb )
		{
			// `placeholderCount` is also the nth string number.

			Random rng = new Random( Seed: rngSeed );

			Random rngLipsum = new Random( Seed: rngSeed );

			try
			{
				// Prefix format string with `n` and `rngSeed` for reference:
				_ = sb.AppendFormat( "(n: {0:N0}, rngSeed: {1:d}) ", n, rngSeed );

				for( Int32 i = 0; i < valueCount; i++ )
				{
					// Append random text literals between placeholders:
					Lipsum.AppendLipsumSubstring( sb, rngLipsum );

					_ = sb.Append( ' ' );

					if( types[i] == typeof(void) )
					{
						AppendCompositePlaceholder(
							sb   : sb,
							index: i,
							align: null,
							fmt  : null
						);
					}
					else if( types[i] == typeof(String) )
					{
						AppendCompositePlaceholder(
							sb   : sb,
							index: i,
							align: ( rng.NextDouble() >= 0.50 ) ? ( rng.Next( maxValue: 21 ) ) : (Int32?)null, // 50% chance of rendering composite format alignment
							fmt  : null
						);
					}
					else if( types[i] == typeof(Int32) )
					{
						AppendCompositePlaceholder(
							sb   : sb,
							index: i,
							align: ( rng.NextDouble() >= 0.50 ) ? ( rng.Next( maxValue: 21 ) ) : (Int32?)null, // 50% chance of rendering composite format alignment
							fmt  : ( rng.NextDouble() >= 0.25 ) ? ( GetRandomElement( _i32Fmts, rng ) ) : null  // 75% chance of rendering composite format args
						);
					}
					else throw new InvalidOperationException();

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

		private static Object[] CreateCompositeFormatStringArgs( Int32 n, Int32 valueCount, Int32 rngSeed, IReadOnlyList<Type> types )
		{
			if( valueCount == 0 ) return Array.Empty<Object>();

			Random rng = new Random( Seed: rngSeed );

			Object[] compositeFormatArgs = new Object[ valueCount ];

			for( Int32 i = 0; i < valueCount; i++ )
			{
				if( types[i] == typeof(void) )
				{
					compositeFormatArgs[i] = null;
				}
				else if( types[i] == typeof(String) )
				{
					compositeFormatArgs[i] = Lipsum.GetLipsumSubstring( rng );
				}
				else if( types[i] == typeof(Int32) )
				{
					compositeFormatArgs[i] = rng.Next();
				}
				else throw new InvalidOperationException();
			}

			return compositeFormatArgs;
		}

	}
}
