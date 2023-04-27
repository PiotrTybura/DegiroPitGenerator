using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Integrations.Exante.Models;
using Models;
using Models.Operations;

namespace Integrations.Exante.Adapters
{
    public interface IDividendAdapter
    {
        IEnumerable<Dividend> Adapt(List<CsvCashOperation> degiroCashOperations);
    }

    public class DividendAdapter : IDividendAdapter
    {
        private readonly CultureInfo _datesCultureInfo;

        public IEnumerable<Dividend> Adapt(List<CsvCashOperation> exanteCashOperations)
        {
            var dividends = exanteCashOperations.Where(_ => _.OperationType == "DIVIDEND");
            var dividendsTaxes = exanteCashOperations.Where(_ => _.OperationType == "TAX").ToList();
            
            var rollbackTaxes = dividendsTaxes.Where(_ => _.Comment.StartsWith("Rollback for transaction "));
            var taxCorrections = dividendsTaxes.Where(_ => _.Comment.StartsWith("Tax correction to "));
            var adjustments = dividendsTaxes.Where(_ => _.Comment.StartsWith("US tax adjustment for Q1 2022"));
            dividendsTaxes = dividendsTaxes.Except(rollbackTaxes).Except(taxCorrections).Except(adjustments).ToList();

            foreach(var rollbackTax in rollbackTaxes)
            {
                var match = Regex.Match(rollbackTax.Comment, @"Rollback for transaction #(\d+)");
                var transactionIdToCorrect = match.Groups[1].Value;
                var transactionToCorrect = dividendsTaxes.Single(_ => _.TransactionId == transactionIdToCorrect);
                transactionToCorrect.Sum += rollbackTax.Sum;
                transactionToCorrect.EurEquivalent += rollbackTax.EurEquivalent;
            }

            foreach(var taxCorrection in taxCorrections)
            {
                var match = Regex.Match(taxCorrection.Comment, @"Tax correction to #(\d+)");
                var transactionIdToCorrect = match.Groups[1].Value;
                var transactionToCorrect = dividendsTaxes.Single(_ => _.TransactionId == transactionIdToCorrect);
                transactionToCorrect.Sum += taxCorrection.Sum;
                transactionToCorrect.EurEquivalent += taxCorrection.EurEquivalent;
            }

            //To remove duplicated dividends (manually paid twice and then corrected)
            dividends = dividends.Except(dividends.Where(_ => _.Sum == 0m)).ToList();
            dividendsTaxes = dividendsTaxes.Except(dividendsTaxes.Where(_ => _.Sum == 0m)).ToList();

            foreach (var dividend in dividends)
            {
                var dividendTax = dividendsTaxes.SingleOrDefault(_ => _.Comment == dividend.Comment);
               
                yield return new Dividend
                {
                    Amount = dividend.Sum.Value,
                    Currency = Enum.Parse<Currency>(dividend.Asset),
                    Date = DateTime.Parse(dividend.When),
                    //There are no stock names in cashOperation reports, so reference must base only on Isin
                    FinancialInstrumentReference = dividend.SymbolId,
                    FinancialInstrumentCommonName = dividend.SymbolId,
                    PaidTaxAmount = -1 * dividendTax?.Sum.Value ?? 0m
                };

                dividendsTaxes.Remove(dividendTax);
            }

            if (dividendsTaxes.Any())
                throw new ArgumentException("Some dividend taxes cannot be mapped to any dividend");
        }
    }
}
