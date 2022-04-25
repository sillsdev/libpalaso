namespace Tenuto.Grammar {

public interface ExpVisitor {
	object OnGroup( GroupExp exp );
	object OnChoice( ChoiceExp exp );
	object OnInterleave( InterleaveExp exp );
	object OnOneOrMore( OneOrMoreExp exp );
	object OnList( ListExp exp );
	object OnMixed( MixedExp exp );
	object OnElement( ElementExp exp );
	object OnAttribute( AttributeExp exp );
	object OnData( DataExp exp );
	object OnValue( ValueExp exp );
	object OnRef( ReferenceExp exp );
	object OnEmpty();
	object OnNotAllowed();
	object OnText();
}

public interface ExpVisitorExp {
	Expression OnGroup( GroupExp exp );
	Expression OnChoice( ChoiceExp exp );
	Expression OnInterleave( InterleaveExp exp );
	Expression OnOneOrMore( OneOrMoreExp exp );
	Expression OnList( ListExp exp );
	Expression OnMixed( MixedExp exp );
	Expression OnElement( ElementExp exp );
	Expression OnAttribute( AttributeExp exp );
	Expression OnData( DataExp exp );
	Expression OnValue( ValueExp exp );
	Expression OnRef( ReferenceExp exp );
	Expression OnEmpty();
	Expression OnNotAllowed();
	Expression OnText();
}

public interface ExpVisitorVoid {
	void OnGroup( GroupExp exp );
	void OnChoice( ChoiceExp exp );
	void OnInterleave( InterleaveExp exp );
	void OnOneOrMore( OneOrMoreExp exp );
	void OnList( ListExp exp );
	void OnMixed( MixedExp exp );
	void OnElement( ElementExp exp );
	void OnAttribute( AttributeExp exp );
	void OnData( DataExp exp );
	void OnValue( ValueExp exp );
	void OnRef( ReferenceExp exp );
	void OnEmpty();
	void OnNotAllowed();
	void OnText();
}

}
