namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal static class ProductCatalogConstants
    {
        public static class ImagePrefix
        {
            public const string Category = $"{BaseImagePrefix}/categories/";
            public const string Product = $"{BaseImagePrefix}/products/";
            private const string BaseImagePrefix = "https://storage.googleapis.com/ecommerceapp/images/";
        }

        public static class CategoryNames
        {
            public static class Root
            {
                public const string GroceriesCategoryName = "Groceries";
                public const string HouseholdCategoryName = "Household";
                public const string ElectronicsCategoryName = "Electronics";
                public const string ClothingCategoryName = "Clothing";
            }

            public static class Groceries
            {
                public const string FruitsAndVegetables = "Fruits & Vegetables";
                public const string Drinks = "Drinks";
                public const string Meat = "Meat & Seafood";
                public const string Bakery = "Bakery";
                public const string Dairy = "Dairy & Eggs";
            }

            public static class Household
            {
                public const string Cleaning = "Cleaning";
                public const string Kitchenware = "Kitchenware";
                public const string Laundry = "Laundry";
            }

            public static class Electronics
            {
                public const string PhonesAndTablets = "Phones & Tablets";
                public const string Appliances = "Appliances";
                public const string Computers = "Computers";
            }

            public static class Clothing
            {
                public const string Mens = "Men's Clothing";
                public const string Womens = "Women's Clothing";
                public const string Kids = "Kids' Clothing";
            }
        }

        public static class ProductNames
        {
            public static class Bakery
            {
                public const string Croissant = "Butter Croissants 4pk";
                public const string Bread = "Wholemeal Bread 800g";
            }

            public static class Dairy
            {
                public const string Milk = "Semi-Skimmed Milk 2L";
                public const string Egg = "Free Range Eggs 12pk";
                public const string Cheese = "Mature Cheddar 400g";
            }

            public static class Cleaning
            {
                public const string Cleaner = "All-Purpose Cleaner 1L";
                public const string DishwasherTablets = "Dishwasher Tablets 40pk";
            }

            public static class Computer
            {
                public const string Laptop = "15.6\" Laptop";
            }

            public static class Drink
            {
                public const string Water = "Spring Water 6x1.5L";
                public const string Cola = "Cola 2L";
            }

            public static class FruitesAndVegetables
            {
                public const string Apple = "Gala Apples 1kg";
                public const string Banana = "Bananas 5pk";
                public const string Orange = "Navel Oranges 4pk";
            }

            public static class Kitchenware
            {
                public const string FryingPan = "Non-Stick Frying Pan 28cm";
            }

            public static class Laundry
            {
                public const string Detergent = "Laundry Detergent 2L";
            }

            public static class Meat
            {
                public const string ChickenBreast = "Chicken Breast Fillets 500g";
                public const string BeefMince = "Lean Beef Mince 500g";
            }

            public static class PhonesAndTablets
            {
                public const string AndroidTablet = "10\" Android Tablet";
            }
        }
    }
}
