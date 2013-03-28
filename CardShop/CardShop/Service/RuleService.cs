﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Workflow.Activities.Rules;
using System.Workflow.ComponentModel.Serialization;
using System.Xml;
using CardShop.Daos;
using CardShop.ViewModels;

namespace CardShop.Service
{
    public class RuleService
    {
        public IPracticeGDVPDao db { get; set; }
        public System.Workflow.Activities.Rules.RuleSet templateRuleset { get; set; }

        public RuleService()
        {
            this.db = PracticeGDVPDao.GetInstance();
            this.templateRuleset = SetUpTemplate();
        }

        private System.Workflow.Activities.Rules.RuleSet SetUpTemplate()
        {
            Models.RuleSet rulesetWrapper = new Models.RuleSet();
            rulesetWrapper.RuleSet1 = "<RuleSet Description=\"{p1:Null}\" Name=\"Template\" ChainingBehavior=\"Full\" xmlns:p1=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/workflow\">   <RuleSet.Rules>    <Rule Priority=\"0\" ReevaluationBehavior=\"Always\" Description=\"{p1:Null}\" Active=\"True\" Name=\"Test1\"><Rule.Condition><RuleExpressionCondition Name=\"{p1:Null}\"><RuleExpressionCondition.Expression><ns0:CodeBinaryOperatorExpression Operator=\"ValueEquality\" xmlns:ns0=\"clr-namespace:System.CodeDom;Assembly=System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"><ns0:CodeBinaryOperatorExpression.Right><ns0:CodePrimitiveExpression><ns0:CodePrimitiveExpression.Value><ns1:String xmlns:ns1=\"clr-namespace:System;Assembly=mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">1</ns1:String></ns0:CodePrimitiveExpression.Value></ns0:CodePrimitiveExpression></ns0:CodeBinaryOperatorExpression.Right><ns0:CodeBinaryOperatorExpression.Left><ns0:CodeFieldReferenceExpression FieldName=\"TestId\"><ns0:CodeFieldReferenceExpression.TargetObject><ns0:CodeThisReferenceExpression /></ns0:CodeFieldReferenceExpression.TargetObject></ns0:CodeFieldReferenceExpression></ns0:CodeBinaryOperatorExpression.Left></ns0:CodeBinaryOperatorExpression></RuleExpressionCondition.Expression></RuleExpressionCondition></Rule.Condition><Rule.ThenActions><RuleStatementAction><RuleStatementAction.CodeDomStatement><ns0:CodeAssignStatement LinePragma=\"{p1:Null}\" xmlns:ns0=\"clr-namespace:System.CodeDom;Assembly=System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"><ns0:CodeAssignStatement.Left><ns0:CodeFieldReferenceExpression FieldName=\"Field\"><ns0:CodeFieldReferenceExpression.TargetObject><ns0:CodeThisReferenceExpression /></ns0:CodeFieldReferenceExpression.TargetObject></ns0:CodeFieldReferenceExpression></ns0:CodeAssignStatement.Left><ns0:CodeAssignStatement.Right><ns0:CodePrimitiveExpression><ns0:CodePrimitiveExpression.Value><ns1:String xmlns:ns1=\"clr-namespace:System;Assembly=mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">1</ns1:String></ns0:CodePrimitiveExpression.Value></ns0:CodePrimitiveExpression></ns0:CodeAssignStatement.Right></ns0:CodeAssignStatement></RuleStatementAction.CodeDomStatement></RuleStatementAction><RuleStatementAction><RuleStatementAction.CodeDomStatement><ns0:CodeAssignStatement LinePragma=\"{p1:Null}\" xmlns:ns0=\"clr-namespace:System.CodeDom;Assembly=System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"><ns0:CodeAssignStatement.Left><ns0:CodeFieldReferenceExpression FieldName=\"Discount\"><ns0:CodeFieldReferenceExpression.TargetObject><ns0:CodeThisReferenceExpression /></ns0:CodeFieldReferenceExpression.TargetObject></ns0:CodeFieldReferenceExpression></ns0:CodeAssignStatement.Left><ns0:CodeAssignStatement.Right><ns0:CodePrimitiveExpression><ns0:CodePrimitiveExpression.Value><ns1:String xmlns:ns1=\"clr-namespace:System;Assembly=mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\">50</ns1:String></ns0:CodePrimitiveExpression.Value></ns0:CodePrimitiveExpression></ns0:CodeAssignStatement.Right></ns0:CodeAssignStatement></RuleStatementAction.CodeDomStatement></RuleStatementAction></Rule.ThenActions></Rule></RuleSet.Rules></RuleSet>";
            System.Workflow.Activities.Rules.RuleSet ruleset = DeserializeRules(rulesetWrapper.RuleSet1);
            return ruleset;
        }

        public Models.RuleSet Create(Models.RuleSet rulesetWrapper)
        {
            rulesetWrapper.ModifiedDate = DateTime.Now;
            rulesetWrapper.MajorVersion = 1;
            rulesetWrapper.MinorVersion = 0;

            string updatedRules = rulesetWrapper.RuleSet1;
            List<RuleObject> rulesObject = DeserializeJSONRulesObject(updatedRules);

            System.Workflow.Activities.Rules.RuleSet ruleset = templateRuleset.Clone();
            ruleset.Name = rulesetWrapper.Name;

            rulesetWrapper.RuleSet1 = CompileRuleset(ruleset, rulesObject);

            db.RuleSets().Add(rulesetWrapper);
            db.SaveChanges();

            return rulesetWrapper;
        }

        public Models.RuleSet Edit(Models.RuleSet rulesetWrapper)
        {
            string rules = rulesetWrapper.RuleSet1;
            List<RuleObject> rulesObject = DeserializeJSONRulesObject(rules);

            string serializedRules = CompileRuleset(templateRuleset, rulesObject);
            rulesetWrapper.RuleSet1 = serializedRules;

            rulesetWrapper.ModifiedDate = DateTime.Now;

            //db.Entry(rulesetWrapper).State = EntityState.Modified;
            Models.RuleSet dbRuleset = db.RuleSets().Where(p => p.RuleSetId == rulesetWrapper.RuleSetId).FirstOrDefault();
            dbRuleset = rulesetWrapper;
            db.SaveChanges();

            return rulesetWrapper;
        }

        public System.Workflow.Activities.Rules.RuleSet DeserializeRules(string rules)
        {
            WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
            System.Workflow.Activities.Rules.RuleSet ruleset = (System.Workflow.Activities.Rules.RuleSet)serializer.Deserialize(XmlReader.Create(new StringReader(rules)));
            return ruleset;
        }

        private string SerializeRuleSet(System.Workflow.Activities.Rules.RuleSet ruleset)
        {
            WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
            StringBuilder ruleDefinition = new StringBuilder();

            if (ruleset != null)
            {
                try
                {
                    StringWriter stringWriter = new StringWriter(ruleDefinition);
                    XmlTextWriter writer = new XmlTextWriter(stringWriter);
                    serializer.Serialize(writer, ruleset);
                    writer.Flush();
                    writer.Close();
                    stringWriter.Flush();
                    stringWriter.Close();
                }
                catch (Exception ex)
                {
                }
            }

            return ruleDefinition.ToString();
        }

        private string CompileRuleset(System.Workflow.Activities.Rules.RuleSet ruleset, List<RuleObject> rulesObject)
        {
            System.Workflow.Activities.Rules.Rule rule = ruleset.Rules.ElementAt(0).Clone();
            RuleStatementAction action = (RuleStatementAction)rule.ThenActions.ElementAt(0).Clone();

            ruleset.Rules.Clear();

            foreach (var ruleObj in rulesObject)
            {
                System.Workflow.Activities.Rules.Rule thisRule = SetUpRule(rule, action, ruleObj);
                ruleset.Rules.Add(thisRule);
            }

            RulesetDetails rulesetDetails = new RulesetDetails();
            return SerializeRuleSet(ruleset);
        }

        private System.Workflow.Activities.Rules.Rule SetUpRule(System.Workflow.Activities.Rules.Rule rule, RuleStatementAction action, RuleObject ruleObj)
        {
            System.Workflow.Activities.Rules.Rule thisRule = rule.Clone();
            thisRule.ThenActions.Clear();
            thisRule.ElseActions.Clear();

            thisRule.Name = ruleObj.Name;

            thisRule.Condition = SetRuleCondition((RuleExpressionCondition)thisRule.Condition, ruleObj.Condition);

            foreach (var actionObj in ruleObj.ThenActions)
            {
                RuleStatementAction thisAction = (RuleStatementAction)action.Clone();
                thisAction = SetRuleAction(thisAction, actionObj);
                thisRule.ThenActions.Add(thisAction);
            }

            foreach (var actionObj in ruleObj.ElseActions)
            {
                RuleStatementAction thisAction = (RuleStatementAction)action.Clone();
                thisAction = SetRuleAction(thisAction, actionObj);
                thisRule.ElseActions.Add(thisAction);
            }

            return thisRule;
        }

        private RuleExpressionCondition SetRuleCondition(RuleExpressionCondition condition, ConditionObject conditionObj)
        {
            CodeBinaryOperatorExpression operatorExpression = (CodeBinaryOperatorExpression)condition.Expression;

            CodeFieldReferenceExpression leftExpression = (CodeFieldReferenceExpression)operatorExpression.Left;
            leftExpression.FieldName = conditionObj.field;

            CodePrimitiveExpression rightExpression = (CodePrimitiveExpression)operatorExpression.Right;
            rightExpression.Value = conditionObj.value;

            return condition;
        }

        private RuleStatementAction SetRuleAction(RuleStatementAction action, ActionObject actionObj)
        {
            CodeAssignStatement domDataValue = (CodeAssignStatement)action.CodeDomStatement;

            CodeFieldReferenceExpression expressionField = (CodeFieldReferenceExpression)domDataValue.Left;
            expressionField.FieldName = actionObj.field;

            CodePrimitiveExpression expressionValue = (CodePrimitiveExpression)domDataValue.Right;
            expressionValue.Value = actionObj.value;

            return action;
        }

        private List<RuleObject> DeserializeJSONRulesObject(string JSON)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<RuleObject> rulesObject = serializer.Deserialize<List<RuleObject>>(JSON);
            return rulesObject;
        }
    }
}