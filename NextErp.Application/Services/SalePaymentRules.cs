namespace NextErp.Application.Services
{
    public static class SalePaymentRules
    {
        public static void RequirePositiveAmount(decimal amount)
        {
            if (amount <= 0)
                throw new InvalidOperationException("Payment amount must be greater than zero.");
        }

        public static void RequireNotOverSaleTotal(decimal saleFinalAmount, decimal alreadyPaid, decimal newAmount)
        {
            var totalAfter = alreadyPaid + newAmount;
            if (totalAfter > saleFinalAmount)
            {
                throw new InvalidOperationException(
                    $"Total payments cannot exceed the sale total of {saleFinalAmount:F2}. " +
                    $"Already recorded: {alreadyPaid:F2}, new payment: {newAmount:F2}.");
            }
        }
    }
}
