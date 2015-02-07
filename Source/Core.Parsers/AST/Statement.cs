/*

 Copyright (c) 2004-2006 Tomas Matousek, Vaclav Novak and Martin Maly.

 The use and distribution terms for this software are contained in the file named License.txt, 
 which can be found in the root of the Phalanger distribution. By using this software 
 in any fashion, you are agreeing to be bound by the terms of this license.
 
 You must not remove this notice from this software.

*/


using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Diagnostics;
using PHP.Core.Parsers;

namespace PHP.Core.AST
{
    #region Statement

    /// <summary>
    /// Abstract base class representing all statements elements of PHP source file.
    /// </summary>
    [Serializable]
    public abstract class Statement : LangElement
    {
        protected Statement(Position position)
            : base(position)
        {
        }

        /// <summary>
        /// Whether the statement is a declaration statement (class, function, namespace, const).
        /// </summary>
        internal virtual bool IsDeclaration { get { return false; } }

        internal virtual bool SkipInPureGlobalCode() { return false; }
    }

    #endregion

    #region BlockStmt

    /// <summary>
    /// Block statement.
    /// </summary>
    [Serializable]
    public sealed class BlockStmt : Statement
    {
        private readonly List<Statement>/*!*/ statements;
        /// <summary>Statements in block</summary>
        public List<Statement>/*!*/ Statements { get { return statements; } }

        public BlockStmt(Position position, List<Statement>/*!*/ body)
            : base(position)
        {
            Debug.Assert(body != null);
            this.statements = body;
        }

        /// <summary>
        /// Call the right Visit* method on the given Visitor object.
        /// </summary>
        /// <param name="visitor">Visitor to be called.</param>
        public override void VisitMe(TreeVisitor visitor)
        {
            visitor.VisitBlockStmt(this);
        }
    }

    #endregion

    #region ExpressionStmt

    /// <summary>
    /// Expression statement.
    /// </summary>
    [Serializable]
    public sealed class ExpressionStmt : Statement
    {
        /// <summary>Expression that repesents this statement</summary>
        public Expression/*!*/ Expression { get { return expression; } internal set { expression = value; } }
        private Expression/*!*/ expression;

        public ExpressionStmt(Position position, Expression/*!*/ expression)
            : base(position)
        {
            Debug.Assert(expression != null);
            this.expression = expression;
        }

        /// <summary>
        /// Call the right Visit* method on the given Visitor object.
        /// </summary>
        /// <param name="visitor">Visitor to be called.</param>
        public override void VisitMe(TreeVisitor visitor)
        {
            visitor.VisitExpressionStmt(this);
        }
    }

    #endregion

    #region EmptyStmt

    /// <summary>
    /// Empty statement.
    /// </summary>
    public sealed class EmptyStmt : Statement
    {
        public static readonly EmptyStmt Unreachable = new EmptyStmt(Position.Invalid);
        public static readonly EmptyStmt Skipped = new EmptyStmt(Position.Invalid);
        public static readonly EmptyStmt PartialMergeResiduum = new EmptyStmt(Position.Invalid);

        internal override bool SkipInPureGlobalCode()
        {
            return true;
        }

        public EmptyStmt(Position p) : base(p) { }

        /// <summary>
        /// Call the right Visit* method on the given Visitor object.
        /// </summary>
        /// <param name="visitor">Visitor to be called.</param>
        public override void VisitMe(TreeVisitor visitor)
        {
            visitor.VisitEmptyStmt(this);
        }
    }

    #endregion

    #region UnsetStmt

    /// <summary>
    /// Represents an <c>unset</c> statement.
    /// </summary>
    [Serializable]
    public sealed class UnsetStmt : Statement
    {
        /// <summary>List of variables to be unset</summary>
        public List<VariableUse> /*!*/VarList { get { return varList; } }
        private readonly List<VariableUse>/*!*/ varList;
        
        public UnsetStmt(Position p, List<VariableUse>/*!*/ varList)
            : base(p)
        {
            Debug.Assert(varList != null);
            this.varList = varList;
        }

        /// <summary>
        /// Call the right Visit* method on the given Visitor object.
        /// </summary>
        /// <param name="visitor">Visitor to be called.</param>
        public override void VisitMe(TreeVisitor visitor)
        {
            visitor.VisitUnsetStmt(this);
        }
    }

    #endregion

    #region GlobalStmt

    /// <summary>
    /// Represents a <c>global</c> statement.
    /// </summary>
    [Serializable]
    public sealed class GlobalStmt : Statement
    {
        public List<SimpleVarUse>/*!*/ VarList { get { return varList; } }
        private List<SimpleVarUse>/*!*/ varList;

        public GlobalStmt(Position p, List<SimpleVarUse>/*!*/ varList)
            : base(p)
        {
            Debug.Assert(varList != null);
            this.varList = varList;
        }

        /// <summary>
        /// Call the right Visit* method on the given Visitor object.
        /// </summary>
        /// <param name="visitor">Visitor to be called.</param>
        public override void VisitMe(TreeVisitor visitor)
        {
            visitor.VisitGlobalStmt(this);
        }
    }

    #endregion

    #region StaticStmt

    /// <summary>
    /// Represents a <c>static</c> statement.
    /// </summary>
    [Serializable]
    public sealed class StaticStmt : Statement
    {
        /// <summary>List of static variables</summary>
        public List<StaticVarDecl>/*!*/ StVarList { get { return stVarList; } }
        private List<StaticVarDecl>/*!*/ stVarList;
        
        public StaticStmt(Position p, List<StaticVarDecl>/*!*/ stVarList)
            : base(p)
        {
            Debug.Assert(stVarList != null);
            this.stVarList = stVarList;
        }

        /// <summary>
        /// Call the right Visit* method on the given Visitor object.
        /// </summary>
        /// <param name="visitor">Visitor to be called.</param>
        public override void VisitMe(TreeVisitor visitor)
        {
            visitor.VisitStaticStmt(this);
        }
    }

    /// <summary>
    /// Helper class. No error or warning can be caused by declaring variable as static.
    /// </summary>
    /// <remarks>
    /// Even this is ok:
    /// 
    /// function f()
    ///	{
    ///   global $a;
    ///   static $a = 1;
    /// }
    /// 
    /// That's why we dont'need to know Position => is not child of LangElement
    /// </remarks>
    [Serializable]
    public class StaticVarDecl : LangElement
    {
        /// <summary>Static variable being declared</summary>
        public DirectVarUse /*!*/ Variable { get { return variable; } }
        private DirectVarUse/*!*/ variable;
        
        /// <summary>Expression used to initialize static variable</summary>
        public Expression Initializer { get { return initializer; } internal set { initializer = value; } }
        private Expression initializer;
        
        public StaticVarDecl(Position position, DirectVarUse/*!*/ variable, Expression initializer)
            : base(position)
        {
            Debug.Assert(variable != null);

            this.variable = variable;
            this.initializer = initializer;
        }

        /// <summary>
        /// Call the right Visit* method on the given Visitor object.
        /// </summary>
        /// <param name="visitor">Visitor to be called.</param>
        public override void VisitMe(TreeVisitor visitor)
        {
            visitor.VisitStaticVarDecl(this);
        }
    }

    #endregion
}
