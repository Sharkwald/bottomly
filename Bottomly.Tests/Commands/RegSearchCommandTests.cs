using Bottomly.Commands;
using Bottomly.Tests.Helpers;
using Moq;
using Shouldly;

namespace Bottomly.Tests.Commands;

public class RegSearchCommandTests
{
    [Fact]
    public async Task ExecuteAsync_EmptyInput_ReturnsRegistrationMissingMessage()
    {
        var command = new RegSearchCommand(new Mock<IHttpClientFactory>().Object);

        var result = await command.ExecuteAsync("   ");

        result.ShouldBe("Registration missing");
    }

    [Fact]
    public async Task ExecuteAsync_TooLongInput_ReturnsTooLongMessage()
    {
        var command = new RegSearchCommand(new Mock<IHttpClientFactory>().Object);

        var result = await command.ExecuteAsync("ABCDEFGH"); // 8 chars

        result.ShouldBe("Registration too long.");
    }

    [Fact]
    public async Task ExecuteAsync_SpecialChars_ReturnsSpecialCharsMessage()
    {
        var command = new RegSearchCommand(new Mock<IHttpClientFactory>().Object);

        var result = await command.ExecuteAsync("AB-12C");

        result.ShouldBe("Registration should not contain special characters");
    }

    [Fact]
    public async Task ExecuteAsync_ValidReg_ParsesHtmlResponse()
    {
        const string html = """
                            <html><body>
                                <input id="VehicleColour" value="Blue" />
                                <input id="RegistrationYear" value="2019" />
                                <input id="VehicleMake" value="FORD" />
                                <input id="VehicleModel" value="FOCUS" />
                                <img id="searchResultCarImage" src="/images/car.jpg" />
                            </body></html>
                            """;

        var factory = TestHelpers.CreateHttpClientFactory(html);
        var command = new RegSearchCommand(factory);

        var result = await command.ExecuteAsync("AB12CDE");

        result.ShouldContain("Blue");
        result.ShouldContain("Ford");
        result.ShouldContain("FOCUS");
        result.ShouldContain("2019");
    }

    [Fact]
    public async Task ExecuteAsync_SpacesInReg_NormalisesBeforeSearch()
    {
        // If spaces are stripped, "AB 12 CDE" becomes "ab12cde" (7 chars, valid)
        const string html = "<html><body><input id=\"VehicleColour\" value=\"Red\" /></body></html>";
        var factory = TestHelpers.CreateHttpClientFactory(html);
        var command = new RegSearchCommand(factory);

        var result = await command.ExecuteAsync("AB 12 CD");

        // Does not return an error about length or special chars
        result.ShouldNotBe("Registration too long.");
        result.ShouldNotBe("Registration should not contain special characters");
    }

    [Fact]
    public async Task ExecuteAsync_HtmlWithError_ReturnsErrorText()
    {
        // An element with an empty value causes make[0] to throw, which triggers the catch block
        const string html = """
                            <html><body>
                                <input id="VehicleMake" value="" />
                                <div class="ErrorMessage"><h3>Vehicle not found</h3></div>
                            </body></html>
                            """;

        var factory = TestHelpers.CreateHttpClientFactory(html);
        var command = new RegSearchCommand(factory);

        var result = await command.ExecuteAsync("ZZ99ZZZ");

        result.ShouldBe("Vehicle not found");
    }
}