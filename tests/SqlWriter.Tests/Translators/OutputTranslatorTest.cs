using SqlWriter.Translators;
using System;
using System.Linq.Expressions;
using SqlWriter.Tests.Fixtures;
using Xunit;

namespace SqlWriter.Tests.Translators;

public class OutputTranslatorTest
{
    private readonly OutputTranslator _feature;

    public OutputTranslatorTest()
    {
        _feature = new();
    }

    [Fact]
    public void Visit_update_output_class_class_with_insert_member()
    {
        string expected = "OUTPUT Inserted.Address, Inserted.PropertyID INTO Table2 (Address, EventID)";
        Expression<Func<QueryableMod1, UpdateOutput<QueryableMod2>>> lambda = (o) => new UpdateOutput<QueryableMod2>()
        {
            Inserted = new QueryableMod2 { Address = o.Address, EventID = o.PropertyID }
        };

        string actual = _feature.Visit(lambda);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Visit_for_generic_class_type()
    {
        string expected = "OUTPUT Inserted.Address, Inserted.PropertyID INTO Table2 (Address, EventID)";
        Expression<Func<QueryableMod1, QueryableMod2>> lambda = (o) => new QueryableMod2() { Address = o.Address, EventID = o.PropertyID };

        string actual = _feature.Visit(lambda);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Visit_update_output_class_class_with_delete_member()
    {
        string expected = "OUTPUT Deleted.Address, Deleted.PropertyID INTO Table2 (Address, EventID)";
        Expression<Func<QueryableMod1, UpdateOutput<QueryableMod2>>> lambda = (o) => new UpdateOutput<QueryableMod2>()
        {
            Deleted = new QueryableMod2 { Address = o.Address, EventID = o.PropertyID }
        };

        string actual = _feature.Visit(lambda);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Visit_update_output_class_class_with_insert_and_delete_members()
    {
        string expected = "OUTPUT Inserted.Address, Inserted.PropertyID, Deleted.Address, Deleted.PropertyID INTO Table2 (Address, EventID, Address, EventID)";
        Expression<Func<QueryableMod1, UpdateOutput<QueryableMod2>>> lambda = (o) => new UpdateOutput<QueryableMod2>()
        {
            Inserted = new QueryableMod2 { Address = o.Address, EventID = o.PropertyID },
            Deleted = new QueryableMod2 { Address = o.Address, EventID = o.PropertyID }
        };

        string actual = _feature.Visit(lambda);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Visit_update_output_class_class_with_insert_and_delete_nullable_members()
    {
        string expected = "OUTPUT Inserted.PcoeDate, Inserted.YesNo, Deleted.PcoeDate, Deleted.YesNo INTO Table4 (PcoeDate, TaskStatus, PcoeDate, TaskStatus)";
        Expression<Func<QueryableMod2, UpdateOutput<QueryableMod4>>> lambda = (o) => new UpdateOutput<QueryableMod4>()
        {
            Inserted = new QueryableMod4 { PcoeDate = o.PcoeDate, TaskStatus = o.YesNo },
            Deleted = new QueryableMod4 { PcoeDate = o.PcoeDate, TaskStatus = o.YesNo }
        };

        string actual = _feature.Visit(lambda);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Visit_for_generic_class_type_with_nullable_types()
    {
        string expected = "OUTPUT Deleted.PcoeDate, Deleted.YesNo INTO Table4 (PcoeDate, TaskStatus)";
        Expression<Func<QueryableMod2, QueryableMod4>> lambda = (o) => new QueryableMod4() { PcoeDate = o.PcoeDate, TaskStatus = o.YesNo };

        string actual = _feature.Visit(lambda, "Deleted");

        Assert.Equal(expected, actual);
    }
}
