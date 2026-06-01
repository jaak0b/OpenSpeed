using FakeItEasy;
using OpenSpeed.Adapter;
using OpenSpeed.Core.Models;
using Z21.Core;
using Z21.Core.Command;
using Z21.Core.Command.Driving;
using Z21.Core.Model;
using CoreDccSpeedMode = OpenSpeed.Core.Models.DccSpeedMode;

namespace OpenSpeed.Adapter.UnitTests;

[TestFixture]
public class Z21ControllerTests
{
    private IZ21Client _z21Client = null!;
    private Z21Controller _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _z21Client = A.Fake<IZ21Client>();
        A.CallTo(() => _z21Client.SendCommandsAsync(A<IZ21Command[]>._)).Returns(Task.CompletedTask);
        _sut = new Z21Controller(_z21Client, new Z21Configuration());
    }

    private IZ21Command? CaptureCommand()
    {
        IZ21Command? captured = null;
        A.CallTo(() => _z21Client.SendCommandsAsync(A<IZ21Command[]>._))
            .Invokes(call => captured = call.GetArgument<IZ21Command[]>(0)?[0])
            .Returns(Task.CompletedTask);
        return captured;
    }

    [Test]
    public async Task SetSpeedAsync_Steps128Mode_SendsSetLocoDriveCommand()
    {
        IZ21Command? captured = null;
        A.CallTo(() => _z21Client.SendCommandsAsync(A<IZ21Command[]>._))
            .Invokes(call => captured = call.GetArgument<IZ21Command[]>(0)?[0])
            .Returns(Task.CompletedTask);

        await _sut.SetSpeedAsync(CoreDccSpeedMode.Steps128, 3, 10, true);

        Assert.That(captured, Is.InstanceOf<SetLocoDriveCommand>());
    }

    [Test]
    public async Task SetSpeedAsync_Steps14Mode_SendsSetLocoDriveCommand()
    {
        IZ21Command? captured = null;
        A.CallTo(() => _z21Client.SendCommandsAsync(A<IZ21Command[]>._))
            .Invokes(call => captured = call.GetArgument<IZ21Command[]>(0)?[0])
            .Returns(Task.CompletedTask);

        await _sut.SetSpeedAsync(CoreDccSpeedMode.Steps14, 3, 5, true);

        Assert.That(captured, Is.InstanceOf<SetLocoDriveCommand>());
    }

    [Test]
    public async Task SetSpeedAsync_Steps28Mode_SendsSetLocoDriveCommand()
    {
        IZ21Command? captured = null;
        A.CallTo(() => _z21Client.SendCommandsAsync(A<IZ21Command[]>._))
            .Invokes(call => captured = call.GetArgument<IZ21Command[]>(0)?[0])
            .Returns(Task.CompletedTask);

        await _sut.SetSpeedAsync(CoreDccSpeedMode.Steps28, 3, 5, true);

        Assert.That(captured, Is.InstanceOf<SetLocoDriveCommand>());
    }

    [Test]
    public async Task SetSpeedAsync_DirectionTrue_CallsSendCommandsAsync()
    {
        await _sut.SetSpeedAsync(CoreDccSpeedMode.Steps128, 3, 10, direction: true);

        A.CallTo(() => _z21Client.SendCommandsAsync(A<IZ21Command[]>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task SetSpeedAsync_DirectionFalse_CallsSendCommandsAsync()
    {
        await _sut.SetSpeedAsync(CoreDccSpeedMode.Steps128, 3, 10, direction: false);

        A.CallTo(() => _z21Client.SendCommandsAsync(A<IZ21Command[]>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void SetSpeedAsync_UnknownDccSpeedMode_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _sut.SetSpeedAsync((CoreDccSpeedMode)999, 3, 10, true));
    }
}
