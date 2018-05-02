using System;
using chen0040.ExpertSystem;
using CsvHelper;
using System.IO;
using System.Collections.Generic;

namespace ArtificialIntelligenceOpdracht1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
			ReadStudentRecords();
        }

		private static void ReadStudentRecords()
		{
			var csv = new CsvReader(File.OpenText("Intakes.csv"));
			var records = csv.GetRecords<StudentInfo>();

			foreach (StudentInfo record in records)
			{
				if(record.gewogen_gemiddelde == "")
				{
					record.gewogen_gemiddelde = "0";
				}

				RuleInferenceEngine rie = GetInferenceEngine();
				rie.AddFact(new IsClause("gewogen_gemiddelde", record.gewogen_gemiddelde));
				rie.AddFact(new IsClause("competenties", record.competenties));
				rie.AddFact(new IsClause("capaciteiten", record.capaciteiten));
				rie.AddFact(new IsClause("intr. Motivatie", record.intr_motivatie));
				rie.AddFact(new IsClause("extr. Motivatie", record.extr_motivatie));
				rie.AddFact(new IsClause("is_mbo_deficient", record.is_mbo_deficient));
				rie.AddFact(new IsClause("Persoonlijk 'bijspijker'-advies", record.persoonlijk_bijspijker_advies));
				rie.AddFact(new IsClause("Aanmelden voor verkort opleidingstraject", record.Aanmelden_voor_verkort_opleidingstraject));
				rie.AddFact(new IsClause("reden_stoppen", record.reden_stoppen));

				List<Clause> unproved_conditions = new List<Clause>();
				var conclusion = rie.Infer("advies", unproved_conditions);

				if(conclusion != null)
				{
					Console.WriteLine("Generated advice");
				}

				//Clause conclusion = null;
				while (conclusion == null)
				{
					conclusion = rie.Infer("advies", unproved_conditions);
					if (conclusion == null)
					{
						if (unproved_conditions.Count == 0)
						{
							break;
						}
						Clause c = unproved_conditions[0];
						Console.WriteLine("ask: " + c + "?");
						unproved_conditions.Clear();
						Console.WriteLine("What is " + c.Variable + "?");
						String value = Console.ReadLine();
						rie.AddFact(new IsClause(c.Variable, value));
					}
				}
			}
		}

		private static RuleInferenceEngine GetInferenceEngine()
		{
			var rie = new RuleInferenceEngine();

			//Put rules here
			var positiveRule = new Rule("Positief");
			positiveRule.AddAntecedent(new GEClause("gewogen_gemiddelde", "16"));
			positiveRule.AddAntecedent(new GEClause("competenties", "6"));
			positiveRule.AddAntecedent(new GEClause("capaciteiten", "6"));
			positiveRule.AddAntecedent(new GEClause("intr. Motivatie", "7"));
			positiveRule.AddAntecedent(new GEClause("extr. Motivatie", "7"));
			positiveRule.AddAntecedent(new IsClause("is_mbo_deficient", "nee"));
			positiveRule.AddAntecedent(new IsClause("Persoonlijk 'bijspijker'-advies", "0"));
			positiveRule.AddAntecedent(new IsClause("Aanmelden voor verkort opleidingstraject", "1"));
			positiveRule.AddAntecedent(new IsClause("reden_stoppen", ""));
			positiveRule.setConsequent(new IsClause("advies", "Positief"));
			rie.AddRule(positiveRule);

			var doubtRule = new Rule("Twijfel");
			doubtRule.AddAntecedent(new GEClause("gewogen_gemiddelde", "12"));
			doubtRule.AddAntecedent(new LEClause("gewogen_gemiddelde", "15"));
			doubtRule.AddAntecedent(new IsClause("competenties", "5"));
			doubtRule.AddAntecedent(new IsClause("capaciteiten", "5"));
			doubtRule.AddAntecedent(new IsClause("intr. Motivatie", "5"));
			doubtRule.AddAntecedent(new IsClause("extr. Motivatie", "5"));
			doubtRule.AddAntecedent(new IsClause("Persoonlijk 'bijspijker'-advies", "1"));
			doubtRule.AddAntecedent(new IsClause("Aanmelden voor verkort opleidingstraject", "0"));
			doubtRule.AddAntecedent(new IsClause("reden_stoppen", "Uitgeschreven voor opleiding."));
			doubtRule.setConsequent(new IsClause("advies", "Twijfel"));
			rie.AddRule(doubtRule);

			var negativeRule = new Rule("Negatief");
			negativeRule.AddAntecedent(new LEClause("gewogen_gemiddelde", "11"));
			negativeRule.AddAntecedent(new LEClause("competenties", "4"));
			negativeRule.AddAntecedent(new LEClause("capaciteiten", "4"));
			negativeRule.AddAntecedent(new LEClause("intr. Motivatie", "4"));
			negativeRule.AddAntecedent(new LEClause("extr. Motivatie", "4"));
			negativeRule.AddAntecedent(new IsClause("Persoonlijk 'bijspijker'-advies", "0"));
			negativeRule.AddAntecedent(new IsClause("reden_stoppen", "Uitgeschreven voor opleiding."));
			negativeRule.setConsequent(new IsClause("advies", "Negatief"));
			rie.AddRule(negativeRule);

			return rie;
		}
    }
}
