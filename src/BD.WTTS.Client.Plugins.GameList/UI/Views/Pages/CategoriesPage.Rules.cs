using System.Globalization;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Layout;
using BD.WTTS.GameList.Categories;
using static BD.WTTS.GameList.Categories.CategoryOperator;

namespace BD.WTTS.UI.Views.Pages;

public partial class CategoriesPage
{
    static readonly CategoryRuleKind[] GroupKinds = [CategoryRuleKind.All, CategoryRuleKind.Any, CategoryRuleKind.Not];

    void RebuildRuleTree()
    {
        RuleTree.Children.Clear();
        if (draft is { } category)
            RuleTree.Children.Add(BuildRule(category.Rules, []));
    }

    Control BuildRule(CategoryRule rule, int[] path)
    {
        var controls = new WrapPanel { ItemSpacing = 6, LineSpacing = 5 };
        var container = new StackPanel { Spacing = 5, Margin = new(0, 3) };
        container.Children.Add(controls);
        if (rule.Kind == CategoryRuleKind.Condition)
        {
            BuildCondition(rule, path, controls);
        }
        else
        {
            var kind = ChoiceBox(GroupKinds, rule.Kind, item => CategoryLabels.Get("Group_" + item), Strings.GameList_Cat_Group,
                value => UpdateRule(path, item =>
                {
                    var children = item.Children;
                    // NOT has one operand. Retain existing rules by grouping them together.
                    if (value == CategoryRuleKind.Not && children.Length > 1)
                        children = [CategoryRule.Group(item.Kind, children)];
                    return CategoryRule.Group(value, children);
                }, rebuild: true));
            controls.Children.Add(kind);
            if (path.Length < CategoryStorage.MaxRuleDepth && (rule.Kind != CategoryRuleKind.Not || rule.Children.Length == 0))
            {
                var addCondition = RuleButton(Strings.GameList_Cat_AddCondition, () => UpdateRule(path,
                    item => item with { Children = [.. item.Children, CategoryRule.Condition("Installed", Equal, "true")] }, rebuild: true));
                var addGroup = RuleButton(Strings.GameList_Cat_AddGroup, () => UpdateRule(path,
                    item => item with { Children = [.. item.Children, CategoryRule.Group(CategoryRuleKind.All)] }, rebuild: true));
                controls.Children.Add(addCondition);
                controls.Children.Add(addGroup);
            }
            var childPanel = new StackPanel { Margin = new(15, 0, 0, 0), Spacing = 3 };
            for (var i = 0; i < rule.Children.Length; i++)
                childPanel.Children.Add(BuildRule(rule.Children[i], [.. path, i]));
            container.Children.Add(childPanel);
        }

        if (path.Length > 0)
        {
            controls.Children.Add(RuleButton(Strings.GameList_Cat_Remove, () =>
                UpdateRule(path[..^1], parent => parent with
                {
                    Children = parent.Children.Where((_, index) => index != path[^1]).ToArray(),
                }, rebuild: true)));
        }
        return container;
    }

    void BuildCondition(CategoryRule rule, int[] path, WrapPanel controls)
    {
        var known = CategoryField.Registry.TryGetValue(rule.Field, out var field);
        IEnumerable<string> fieldIds = CategoryField.Registry.Keys;
        if (!known)
            fieldIds = fieldIds.Prepend(rule.Field);
        controls.Children.Add(ChoiceBox(fieldIds, rule.Field, item => CategoryLabels.Get("Field_" + item), Strings.GameList_Cat_Field,
            id =>
            {
                var selected = CategoryField.Registry[id];
                var values = selected.Kind == CategoryValueKind.Boolean ? new[] { "true" } : Array.Empty<string>();
                UpdateRule(path, _ => CategoryRule.Condition(id, selected.Operators[0], values), rebuild: true);
            }));
        if (field == null)
            return;

        var operators = field.Operators.Contains(rule.Operator) ? field.Operators : field.Operators.Prepend(rule.Operator);
        controls.Children.Add(ChoiceBox(operators, rule.Operator, op => CategoryLabels.Get("Op_" + op),
            Strings.GameList_Cat_Operator, op => UpdateRule(path, item => item with { Operator = op },
                rebuild: true)));
        if (rule.Operator != IsEmpty)
            BuildValue(rule, path, field, controls);
    }

    void BuildValue(CategoryRule rule, int[] path, CategoryField field, WrapPanel controls)
    {
        var value = rule.Values.FirstOrDefault() ?? string.Empty;
        if (field.Kind == CategoryValueKind.Boolean)
        {
            var selected = bool.TryParse(value, out var boolean) ? boolean.ToString().ToLowerInvariant() : string.Empty;
            controls.Children.Add(ChoiceBox(new[] { "true", "false" }, selected, item => CategoryLabels.DisplayValue(field, item),
                Strings.GameList_Cat_Value, item => SetValues(path, [item])));
        }
        else if (field.Choices is { } choices && rule.Operator is In or HasAny or HasAll)
        {
            foreach (var choice in choices)
            {
                var box = new CheckBox
                {
                    Content = CategoryLabels.DisplayValue(field, choice),
                    IsChecked = rule.Values.Contains(choice, StringComparer.OrdinalIgnoreCase),
                };
                box.IsCheckedChanged += (_, _) => UpdateRule(path, item => item with
                {
                    Values = box.IsChecked == true ? [.. item.Values, choice] : item.Values.Where(current => current != choice).ToArray(),
                });
                controls.Children.Add(box);
            }
        }
        else if (field.Choices is { } singleChoices)
        {
            controls.Children.Add(ChoiceBox(singleChoices, value, item => CategoryLabels.DisplayValue(field, item),
                Strings.GameList_Cat_Value, item => SetValues(path, [item])));
        }
        else if (field.Kind == CategoryValueKind.Number || rule.Operator is WithinDays or OlderThanDays)
        {
            var count = rule.Operator == Between ? 2 : 1;
            for (var index = 0; index < count; index++)
                controls.Children.Add(NumberBox(rule, path, index));
        }
        else if (field.Kind == CategoryValueKind.Date)
        {
            controls.Children.Add(DateBox(rule, path, 0));
            if (rule.Operator == Between)
                controls.Children.Add(DateBox(rule, path, 1));
        }
        else if (field.Kind == CategoryValueKind.Tags)
        {
            AddTagControls(rule, path, controls);
        }
        else
        {
            controls.Children.Add(ValueTextBox(rule, path));
        }
    }

    Control NumberBox(CategoryRule rule, int[] path, int index)
    {
        var days = rule.Operator is WithinDays or OlderThanDays;
        var input = new NumericUpDown
        {
            MinWidth = 100,
            MaxWidth = 165,
            Minimum = days ? 0 : decimal.MinValue,
            Maximum = days ? int.MaxValue : decimal.MaxValue,
            Increment = days ? 1 : 0.1m,
            Value = decimal.TryParse(rule.Values.ElementAtOrDefault(index), NumberStyles.Number,
                CultureInfo.InvariantCulture, out var number) ? number : null,
        };
        AutomationProperties.SetName(input, days ? Strings.GameList_Cat_Days : ValueLabel(rule, index));
        input.ValueChanged += (_, _) => SetValue(path, index, input.Value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
        return input;
    }

    Control DateBox(CategoryRule rule, int[] path, int index)
    {
        var input = new DatePicker
        {
            MinWidth = 165,
            SelectedDate = DateOnly.TryParseExact(rule.Values.ElementAtOrDefault(index), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date) ? new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero) : null,
        };
        AutomationProperties.SetName(input, ValueLabel(rule, index));
        input.SelectedDateChanged += (_, _) => SetValue(path, index, input.SelectedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty);
        return input;
    }

    static string ValueLabel(CategoryRule rule, int index) => rule.Operator != Between ? Strings.GameList_Cat_Value :
        index == 0 ? Strings.GameList_Cat_MinValue : Strings.GameList_Cat_MaxValue;

    TextBox ValueTextBox(CategoryRule rule, int[] path)
    {
        var multi = rule.Operator is HasAny or HasAll or In;
        var input = new TextBox
        {
            Width = 200,
            MaxLength = 500,
            PlaceholderText = multi ? Strings.GameList_Cat_ValueList : Strings.GameList_Cat_Value,
            Text = multi ? string.Join(", ", rule.Values) : rule.Values.FirstOrDefault(),
        };
        AutomationProperties.SetName(input, input.PlaceholderText!);
        input.TextChanged += (_, _) => SetValues(path, multi
            ? (input.Text ?? string.Empty).Split([',', '，'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            : [input.Text ?? string.Empty]);
        return input;
    }

    void AddTagControls(CategoryRule rule, int[] path, WrapPanel controls)
    {
        var multi = rule.Operator is HasAny or HasAll;
        var tag = new AutoCompleteBox
        {
            Width = 200,
            MinimumPrefixLength = 1,
            FilterMode = AutoCompleteFilterMode.ContainsOrdinal,
            PlaceholderText = Strings.GameList_Cat_FindTag,
            ItemsSource = ViewModel?.Tags ?? [],
            Text = multi ? null : rule.Values.FirstOrDefault(),
        };
        AutomationProperties.SetName(tag, Strings.GameList_Cat_FindTag);
        if (multi)
        {
            var values = ValueTextBox(rule, path);
            controls.Children.Add(values);
            controls.Children.Add(tag);
            controls.Children.Add(RuleButton(Strings.GameList_Cat_AddTag, () =>
            {
                if (string.IsNullOrWhiteSpace(tag.Text))
                    return;
                values.Text = string.IsNullOrWhiteSpace(values.Text) ? tag.Text.Trim() : $"{values.Text}, {tag.Text.Trim()}";
                tag.Text = string.Empty;
            }));
        }
        else
        {
            controls.Children.Add(tag);
            tag.PropertyChanged += (_, e) =>
            {
                if (e.Property == AutoCompleteBox.TextProperty)
                    SetValues(path, [tag.Text ?? string.Empty]);
            };
        }
    }

    static ComboBox ChoiceBox<T>(IEnumerable<T> values, T selected, Func<T, string> label, string name, Action<T> changed)
    {
        var items = values.Select(value => new Choice<T>(value, label(value))).ToArray();
        var combo = new ComboBox
        {
            MinWidth = 95,
            ItemsSource = items,
            SelectedItem = items.FirstOrDefault(item => EqualityComparer<T>.Default.Equals(item.Value, selected)),
            PlaceholderText = name,
        };
        AutomationProperties.SetName(combo, name);
        combo.SelectionChanged += (_, _) =>
        {
            if (combo.SelectedItem is Choice<T> choice)
                changed(choice.Value);
        };
        return combo;
    }

    static Button RuleButton(string label, Action action)
    {
        var button = new Button { Content = label, VerticalAlignment = VerticalAlignment.Center };
        button.Click += (_, _) => action();
        return button;
    }

    void SetValue(int[] path, int index, string value) => UpdateRule(path, item =>
    {
        var values = new string[Math.Max(item.Values.Length, index + 1)];
        for (var i = 0; i < values.Length; i++)
            values[i] = i == index ? value : item.Values.ElementAtOrDefault(i) ?? string.Empty;
        return item with { Values = values };
    });

    void SetValues(int[] path, string[] values) => UpdateRule(path, item => item with { Values = values });

    void UpdateRule(int[] path, Func<CategoryRule, CategoryRule> change, bool rebuild = false)
    {
        if (draft is not { } category || loadingControls)
            return;
        var updated = ChangeAt(category.Rules, path, change);
        draft = category with { Rules = updated };
        if (rebuild)
            RebuildRuleTree();
        QueuePreview();
    }

    static CategoryRule ChangeAt(CategoryRule item, ReadOnlySpan<int> path, Func<CategoryRule, CategoryRule> change)
    {
        if (path.Length == 0)
            return change(item);
        var children = (CategoryRule[])item.Children.Clone();
        var index = path[0];
        children[index] = ChangeAt(children[index], path[1..], change);
        return item with { Children = children };
    }
}
