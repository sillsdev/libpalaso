namespace Tenuto.Grammar {

using org.relaxng.datatype;
using System;

public sealed class ExpBuilder  {

	public Expression CreateSequence( Expression exp1, Expression exp2 ) {
		
		if( exp1==Expression.Empty )	return exp2;
		if( exp2==Expression.Empty )	return exp1;
		if( exp1==Expression.NotAllowed || exp2==Expression.NotAllowed )
			return Expression.NotAllowed;

		if( exp2 is GroupExp ) {
			// operators are left-associative.
			GroupExp right = (GroupExp)exp2;
			return CreateSequence( CreateSequence(exp1,right.exp1), right.exp2 );
		}
		
		return Unify( new GroupExp(exp1,exp2) );
	}

	public Expression CreateInterleave( Expression exp1, Expression exp2 ) {
		
		if( exp1==Expression.Empty )	return exp2;
		if( exp2==Expression.Empty )	return exp1;
		if( exp1==Expression.NotAllowed || exp2==Expression.NotAllowed )
			return Expression.NotAllowed;

		if( exp2 is InterleaveExp ) {
			// operators are left-associative.
			InterleaveExp right = (InterleaveExp)exp2;
			return CreateInterleave( CreateInterleave(exp1,right.exp1), right.exp2 );
		}
		
		return Unify( new InterleaveExp(exp1,exp2) );
	}
	
	public Expression CreateChoice( Expression[] exps, int len ) {
		return CreateChoice( exps, 0, len );
	}

	public Expression CreateChoice( Expression[] exps, int start, int len ) {
		Expression e = Expression.NotAllowed;
		for( int i=start; i<len; i++ )
			e = CreateChoice( e, exps[i] );
		return e;
	}

	public Expression CreateChoice( Expression exp1, Expression exp2 ) {
		
		if( exp1==Expression.NotAllowed )	return exp2;
		if( exp2==Expression.NotAllowed )	return exp1;
		
		if( exp1==Expression.Empty && exp2.IsNullable)	return exp2;
		else
		if( exp2==Expression.Empty && exp1.IsNullable)	return exp1;
		
		if( exp2 is ChoiceExp ) {
			// operatos are left-associative.
			ChoiceExp right = (ChoiceExp)exp2;
			return CreateChoice( CreateChoice(exp1,right.exp1), right.exp2 );
		}
		
		// see if exp1 has already contained exp2
		Expression e = exp1;
		while( true ) {
			if(e==exp2)
				// exp1 has already contained exp2.
				return exp1;
			if(!(e is ChoiceExp))
				break;
			ChoiceExp left = (ChoiceExp)e;
			if(left.exp2==exp2)
				// exp1 has already contained exp2.
				return exp1;
			e = left.exp1;
		}
		
		return Unify(new ChoiceExp(exp1,exp2));
	}
	
	public Expression CreateOneOrMore( Expression exp ) {
		if( exp==Expression.NotAllowed
		||  exp==Expression.Empty
		||  exp==Expression.Text
		||  exp is OneOrMoreExp )
			return exp;
		
		return Unify(new OneOrMoreExp(exp));
	}
	
	public Expression CreateZeroOrMore( Expression exp ) {
		return CreateOptional(CreateOneOrMore(exp));
	}
	
	public Expression CreateOptional( Expression exp ) {
		return CreateChoice(exp,Expression.Empty);
	}
	
	public Expression CreateList( Expression exp ) {
		if( exp==Expression.NotAllowed
		||  exp==Expression.Empty )
			return exp;
		
		return Unify(new ListExp(exp));
	}
	
	public Expression CreateData( Datatype dt, Expression except, string typeName ) {
		return Unify(new DataExp(dt,except,typeName));
	}
	
	public Expression CreateValue( Datatype dt, Object value ) {
		return Unify(new ValueExp(dt,value));
	}
	
	public Expression CreateMixed( Expression exp ) {
		if(exp==Expression.Empty)			return Expression.Text;
		if(exp==Expression.NotAllowed)		return Expression.NotAllowed;
		return Unify(new MixedExp(exp));
	}
	
	// hash table
	private Expression[] table = null;
	private const int INIT_SIZE = 256;
	private const float LOAD_FACTOR = 0.3f;
	private int used=0;
	private int threshold;
	
	
	private Expression Unify( Expression exp ) {
		int idx;
		
		if( table==null ) {
			table = new Expression[INIT_SIZE];
			threshold = (int)(INIT_SIZE*LOAD_FACTOR);
			idx = firstIndex(exp);
		} else {
			for( idx=firstIndex(exp); table[idx]!=null; idx=nextIndex(idx,exp) ) {
				if( table[idx].Equals(exp) )
					return table[idx];	// the same expression is found.
			}
		}
		
		used++;
		table[idx] = exp;
		
		if( used>threshold ) {// rehash
			Expression[] old = table;
			table = new Expression[old.Length*2];
			for( int i=old.Length-1; i>=0; i-- ) {
				Expression item = old[i];
				if(item==null)	continue;
				
				int j;
				for( j=firstIndex(item); table[j]!=null; j=nextIndex(j,item) )
					;
				table[j]=item;
			}
			threshold = (int)(table.Length*LOAD_FACTOR);
		}
		
		return exp;
	}
	
	private int firstIndex( Expression exp ) {
		return exp.GetHashCode() & (table.Length-1);
	}
	
	private int nextIndex( int idx, Expression exp ) {
		idx--;
		if(idx<0)	return table.Length-1;
		return idx;
	}
}
}
