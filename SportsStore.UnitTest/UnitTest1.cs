using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Entities;
using System.Linq;
using SportsStore.WebUI.Controllers;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;
using SportsStore.WebUI.Models;
using SportsStore.WebUI.HtmlHelpers;

namespace SportsStore.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        private Mock<IProductsRepository> InitializeMock()
        {
            Mock<IProductsRepository> mockObject = new Mock<IProductsRepository>();
            mockObject.Setup(m => m.Products).Returns(new Product[]
            {
                new Product {ProductID = 1, Name = "P1", Category = "Apples"},
                new Product {ProductID = 2, Name = "P2", Category = "Apples"},
                new Product {ProductID = 3, Name = "P3", Category = "Plums"},
                new Product {ProductID = 4, Name = "P4", Category = "Oranges"},
                new Product {ProductID = 5, Name = "P5", Category = "Oranges"}
            }.AsQueryable());

            return mockObject;
        }
        [TestMethod]
        public void Paginate()
        {
            // arrange
            Mock<IProductsRepository> mock = InitializeMock();

            ProductController controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            // Act
            ProductsListViewModel result = (ProductsListViewModel)controller.List(null, 2).Model;

            // assert
            Product[] prodArray = result.Products.ToArray();
            Assert.IsTrue(prodArray.Length == 2);
            Assert.AreEqual(prodArray[0].Name, "P4");
            Assert.AreEqual(prodArray[1].Name, "P5");

        }

        [TestMethod]
        public void Can_Generate_Page_Links()
        {
            // Arrange - define an HTML helper - we need to do this
            // in order to apply the extension method
            HtmlHelper helper = null;

            // arrange - create PageInfo data
            PagingInfo pagingInfo = new PagingInfo
            {
                CurrentPage = 2,
                TotalItems = 28,
                ItemsPerPage = 10
            };

            // arrange - setup the delegate using lambda
            Func<int, string> pageurlDelegate = i => "Page" + i;

            //Act
            MvcHtmlString result = helper.PageLinks(pagingInfo, pageurlDelegate);

            // assert
            Assert.AreEqual(result.ToString(), @"<a href=""Page1"">1</a>"
                + @"<a class=""selected"" href=""Page2"">2</a>"
                + @"<a href=""Page3"">3</a>");


        }

        [TestMethod]
        public void Can_Send_Pagination_View_Model()
        {
            // arrange
            Mock<IProductsRepository> mock = InitializeMock();

            ProductController controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            // Act 
            ProductsListViewModel result = (ProductsListViewModel)controller.List(null, 2).Model;

            // Assert
            PagingInfo pageInfo = result.PagingInfo;
            Assert.AreEqual(pageInfo.CurrentPage, 2);
            Assert.AreEqual(pageInfo.ItemsPerPage, 3);
            Assert.AreEqual(pageInfo.TotalItems, 5);
            Assert.AreEqual(pageInfo.TotalPages, 2);
        }

        [TestMethod]
        public void Can_Filter_Product()
        {
            // arrange
            Mock<IProductsRepository> mock = InitializeMock();

            ProductController controller = new ProductController(mock.Object);
            controller.PageSize = 3;
            string categoryFilter = "Apples";

            // act
            Product[] result = ((ProductsListViewModel)controller.List(categoryFilter, 1).Model)
                .Products.ToArray();

            // assert
            Assert.AreEqual(result.Length, 2);
            Assert.IsTrue(result[0].Name == "P1" && result[0].Category == categoryFilter);
            Assert.IsTrue(result[1].Name == "P2" && result[1].Category == categoryFilter);
        }

        [TestMethod]
        public void Can_Create_Categories()
        {
            //arrange
            Mock<IProductsRepository> mock = InitializeMock();
            NavController target = new NavController(mock.Object);

            // act
            string[] results = ((IEnumerable<string>)target.Menu().Model).ToArray();

            Assert.AreEqual(results.Length, 3);
            Assert.AreEqual(results[0], "Apples");
            Assert.AreEqual(results[1], "Oranges");
            Assert.AreEqual(results[2], "Plums");
        }

        [TestMethod]
        public void Indicates_Selected_Category()
        {
            // arrange
            var mock = InitializeMock();
            NavController target = new NavController(mock.Object);
            string categoryToSelect = "Apples";

            // act
            string result = target.Menu(categoryToSelect).ViewBag.SelectedCategory;

            //assert
            Assert.AreEqual(categoryToSelect, result);


        }

        [TestMethod]
        public void Generate_Category_Specific_Count()
        {
            // arrange
            var mock = InitializeMock();
            ProductController target = new ProductController(mock.Object);
            target.PageSize = 3;

            // action
            int res1 = ((ProductsListViewModel)target.List("Apples").Model).PagingInfo.TotalItems;
            int res2 = ((ProductsListViewModel)target.List("Oranges").Model).PagingInfo.TotalItems;
            int res3 = ((ProductsListViewModel)target.List("Plums").Model).PagingInfo.TotalItems;
            int resAll = ((ProductsListViewModel)target.List(null).Model).PagingInfo.TotalItems;

            // assert
            Assert.AreEqual(res1, 2);
            Assert.AreEqual(res2, 2);
            Assert.AreEqual(res3, 1);
            Assert.AreEqual(resAll, 5);

        }
    }
}
