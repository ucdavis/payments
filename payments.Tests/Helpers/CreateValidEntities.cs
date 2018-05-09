using Payments.Core.Domain;

namespace payments.Tests.Helpers
{
    public static class CreateValidEntities
    {
        public static Invoice Invoice(int? counter, int lineItemCount = 3, bool loadAll = false)
        {
            var rtValue = new Invoice();
            rtValue.Id = counter ?? 99;
            for (int i = 0; i < lineItemCount; i++)
            {
                rtValue.Items.Add(CreateValidEntities.LineItem(i+1));
            }

            rtValue.CustomerEmail = $"test{counter}@test.com";
            rtValue.CustomerName = $"CustomerName{counter}";
            if (loadAll)
            {
                //Do rest
            }


            return rtValue;
        }

        public static LineItem LineItem(int? counter)
        {
            var rtValue = new LineItem();
            rtValue.Id = counter ?? 99;
            rtValue.Amount = (counter ?? 99);
            rtValue.Quantity = counter ?? 1;
            rtValue.Total = rtValue.Amount * rtValue.Quantity;
            rtValue.Description = $"Description{counter}";

            return rtValue;
        }
    }
}