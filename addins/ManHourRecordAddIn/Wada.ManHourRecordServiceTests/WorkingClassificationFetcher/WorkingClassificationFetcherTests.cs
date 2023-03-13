using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wada.ManHourRecordService.WorkingClassificationFetcher.Tests;

[TestClass()]
public class WorkingClassificationFetcherTests
{
    [DataTestMethod()]
    [DynamicData(nameof(WorkingClassArguments))]
    public async Task 正常系_項目分類が取得できること(object?[][] cellValues, string departmentName, WorkingClassificationRecord expected)
    {
        // given
        // when
        IWorkingClassificationFetcher fetcher = new WorkingClassificationFetcher();
        var workingClass = await fetcher.FetchAsync(cellValues, departmentName);

        // then
        Assert.AreEqual(expected.MajorRange, workingClass.MajorRange);
        CollectionAssert.AreEquivalent(expected.MiddleClassification, workingClass.MiddleClassification);
    }

    private static IEnumerable<object[]> WorkingClassArguments => new List<object[]>
    {
        new object[] {
            new object?[][]
            {
                // 部署間に空行がある
                new object?[] { "項目分類表", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "F13", "F14" },
                new object?[] { "生産管理部", "大分類", "会議", "監査対応", "標準化", "教育", "生産技術", "生産管理", "外注管理", "システム", "事務処理", "現場対応", "他部署支援", "その他" },
                new object?[] { null, "中分類", "社内", "内部監査", "社内標準", "講師", "見積", "進捗管理", "見積照会", "開発", "事務処理", "現場対応", "他部署支援", "プラン" },
                new object?[] { null, null, "社外", "外部監査", "ISO", "受講者", "生産技術(既存)", "発注", "評価", "トラブル対応", null, null, null, "プロジェクト" },
                new object?[] { null, null, null, null, null, "展示会", "生産技術(開発)", null, null, null, null, null, null, "購買管理" },
                new object?[] { null, null, null, null, null, null, null, null, null, null, null, null, null, null },
                new object?[] { "総務部", "大分類", "会議", "監査対応", "標準化", "教育", "生産技術", "生産管理", "外注管理", "システム", "事務処理", "現場対応", "他部署支援", null },
                new object?[] { null, "中分類", "社内", "内部監査", "社内標準", "講師", "見積", "進捗管理", "見積照会", "開発", "事務処理", "現場対応", "他部署支援", null },
                new object?[] { null, null, "社外", "外部監査", "ISO", "受講者", "生産技術(既存)", "発注", "評価", "トラブル対応", null, null, null, null },
                new object?[] { null, null, null, null, null, "展示会", "生産技術(開発)", null, null, null, null, null, null, null },
                new object?[] { null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            },
            "生産管理部",
            new WorkingClassificationRecord(
                new(new(1,2), new(1,13)),
                new()
                {
                    { "会議",       new(new(2, 2),  new(3, 2))},
                    { "監査対応",   new(new(2, 3),  new(3, 3))},
                    { "標準化",     new(new(2, 4),  new(3, 4))},
                    { "教育",       new(new(2, 5),  new(4, 5))},
                    { "生産技術",   new(new(2, 6),  new(4, 6))},
                    { "生産管理",   new(new(2, 7),  new(3, 7))},
                    { "外注管理",   new(new(2, 8),  new(3, 8))},
                    { "システム",   new(new(2, 9), new(3, 9))},
                    { "事務処理",   new(new(2, 10), new(2, 10))},
                    { "現場対応",   new(new(2, 11), new(2, 11))},
                    { "他部署支援", new(new(2, 12), new(2, 12))},
                    { "その他",     new(new(2, 13), new(4, 13)) },
                }),
        },
        new object[] {
            new object?[][]
            {
                // 部署間に空行がない
                new object?[] { "項目分類表", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "F13", "F14" },
                new object?[] { "設計部", "大分類", "会議", "監査対応", "標準化", "教育", "生産技術", "生産管理", "外注管理", "システム", "事務処理", "現場対応", "他部署支援", "その他" },
                new object?[] { null, "中分類", "社内", "内部監査", "社内標準", "講師", "見積", "進捗管理", "見積照会", "開発", "事務処理", "現場対応", "他部署支援", "プラン" },
                new object?[] { null, null, "社外", "外部監査", "ISO", "受講者", "生産技術(既存)", "発注", "評価", "トラブル対応", null, null, null, "プロジェクト" },
                new object?[] { null, null, null, null, null, "展示会", "生産技術(開発)", null, null, null, null, null, null, "購買管理" },
                new object?[] { "総務部", "大分類", "会議", "監査対応", "標準化", "教育", "生産技術", "生産管理", "外注管理", "システム", "事務処理", "現場対応", "他部署支援", null },
                new object?[] { null, "中分類", "社内", "内部監査", "社内標準", "講師", "見積", "進捗管理", "見積照会", "開発", "事務処理", "現場対応", "他部署支援", null },
                new object?[] { null, null, "社外", "外部監査", "ISO", "受講者", "生産技術(既存)", "発注", "評価", "トラブル対応", null, null, null, null },
                new object?[] { null, null, null, null, null, "展示会", "生産技術(開発)", null, null, null, null, null, null, null },
                new object?[] { null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            },
            "設計部",
            new WorkingClassificationRecord(
                new(new(1, 2), new(1, 13)),
                new()
                {
                    { "会議",       new(new(2, 2),  new(3, 2))},
                    { "監査対応",   new(new(2, 3),  new(3, 3))},
                    { "標準化",     new(new(2, 4),  new(3, 4))},
                    { "教育",       new(new(2, 5),  new(4, 5))},
                    { "生産技術",   new(new(2, 6),  new(4, 6))},
                    { "生産管理",   new(new(2, 7),  new(3, 7))},
                    { "外注管理",   new(new(2, 8),  new(3, 8))},
                    { "システム",   new(new(2, 9), new(3, 9))},
                    { "事務処理",   new(new(2, 10), new(2, 10))},
                    { "現場対応",   new(new(2, 11), new(2, 11))},
                    { "他部署支援", new(new(2, 12), new(2, 12))},
                    { "その他",     new(new(2, 13), new(4, 13)) },
                }),
        },
        new object[] {
            new object?[][]
            {
                // 最後の部署で末尾に空行がある
                new object?[] { "項目分類表", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "F13", "F14" },
                new object?[] { "生産管理部", "大分類", "会議", "監査対応", "標準化", "教育", "生産技術", "生産管理", "外注管理", "システム", "事務処理", "現場対応", "他部署支援", "その他" },
                new object?[] { null, "中分類", "社内", "内部監査", "社内標準", "講師", "見積", "進捗管理", "見積照会", "開発", "事務処理", "現場対応", "他部署支援", "プラン" },
                new object?[] { null, null, "社外", "外部監査", "ISO", "受講者", "生産技術(既存)", "発注", "評価", "トラブル対応", null, null, null, "プロジェクト" },
                new object?[] { null, null, null, null, null, "展示会", "生産技術(開発)", null, null, null, null, null, null, "購買管理" },
                new object?[] { null, null, null, null, null, null, null, null, null, null, null, null, null, null },
                new object?[] { "総務部", "大分類", "会議", "監査対応", "標準化", "教育", "生産技術", "生産管理", "外注管理", "システム", "事務処理", "現場対応", "他部署支援", null },
                new object?[] { null, "中分類", "社内", "内部監査", "社内標準", "講師", "見積", "進捗管理", "見積照会", "開発", "事務処理", "現場対応", "他部署支援", null },
                new object?[] { null, null, "社外", "外部監査", "ISO", "受講者", "生産技術(既存)", "発注", "評価", "トラブル対応", null, null, null, null },
                new object?[] { null, null, null, null, null, "展示会", "生産技術(開発)", null, null, null, null, null, null, null },
                new object?[] { null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            },
            "総務部",
            new WorkingClassificationRecord(
                new(new(6, 2), new(6, 12)),
                new()
                {
                    { "会議",       new(new(7, 2),  new(8, 2))},
                    { "監査対応",   new(new(7, 3),  new(8, 3))},
                    { "標準化",     new(new(7, 4),  new(8, 4))},
                    { "教育",       new(new(7, 5),  new(9, 5))},
                    { "生産技術",   new(new(7, 6),  new(9, 6))},
                    { "生産管理",   new(new(7, 7),  new(8, 7))},
                    { "外注管理",   new(new(7, 8),  new(8, 8))},
                    { "システム",   new(new(7, 9),  new(8, 9))},
                    { "事務処理",   new(new(7, 10), new(7, 10))},
                    { "現場対応",   new(new(7, 11), new(7, 11))},
                    { "他部署支援", new(new(7, 12), new(7, 12))},
                }),
        },
        new object[] {
            new object?[][]
            {
                // 最後の部署で末尾に空行がない
                new object?[] { "項目分類表", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "F13", "F14" },
                new object?[] { "生産管理部", "大分類", "会議", "監査対応", "標準化", "教育", "生産技術", "生産管理", "外注管理", "システム", "事務処理", "現場対応", "他部署支援", "その他" },
                new object?[] { null, "中分類", "社内", "内部監査", "社内標準", "講師", "見積", "進捗管理", "見積照会", "開発", "事務処理", "現場対応", "他部署支援", "プラン" },
                new object?[] { null, null, "社外", "外部監査", "ISO", "受講者", "生産技術(既存)", "発注", "評価", "トラブル対応", null, null, null, "プロジェクト" },
                new object?[] { null, null, null, null, null, "展示会", "生産技術(開発)", null, null, null, null, null, null, "購買管理" },
                new object?[] { null, null, null, null, null, null, null, null, null, null, null, null, null, null },
                new object?[] { "経理部", "大分類", "会議", "監査対応", "標準化", "教育", "生産技術", "生産管理", "外注管理", "システム", "事務処理", "現場対応", "他部署支援", null },
                new object?[] { null, "中分類", "社内", "内部監査", "社内標準", "講師", "見積", "進捗管理", "見積照会", "開発", "事務処理", "現場対応", "他部署支援", null },
                new object?[] { null, null, "社外", "外部監査", "ISO", "受講者", "生産技術(既存)", "発注", "評価", "トラブル対応", null, null, null, null },
                new object?[] { null, null, null, null, null, "展示会", "生産技術(開発)", null, null, null, null, null, null, null },
            },
            "経理部",
            new WorkingClassificationRecord(
                new(new(6, 2), new(6, 12)),
                new()
                {
                    { "会議",       new(new(7, 2),  new(8, 2))},
                    { "監査対応",   new(new(7, 3),  new(8, 3))},
                    { "標準化",     new(new(7, 4),  new(8, 4))},
                    { "教育",       new(new(7, 5),  new(9, 5))},
                    { "生産技術",   new(new(7, 6),  new(9, 6))},
                    { "生産管理",   new(new(7, 7),  new(8, 7))},
                    { "外注管理",   new(new(7, 8),  new(8, 8))},
                    { "システム",   new(new(7, 9),  new(8, 9))},
                    { "事務処理",   new(new(7, 10), new(7, 10))},
                    { "現場対応",   new(new(7, 11), new(7, 11))},
                    { "他部署支援", new(new(7, 12), new(7, 12))},
                }),
        },
    };
}