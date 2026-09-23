import 'package:flutter/material.dart';

import '../core/theme/app_colors.dart';
import '../core/theme/app_spacing.dart';
import '../models/search_result.dart';
import 'api_image.dart';
import 'disabled_action.dart';

typedef PageLoader<T> =
    Future<SearchResult<T>> Function(
      int page,
      int pageSize,
      String? search,
      String? sortBy,
    );

class CrudColumn<T> {
  const CrudColumn({
    required this.label,
    required this.value,
    this.sortKey,
    this.isImage = false,
    this.maxWidth,
  });

  final String label;
  final String Function(T item) value;
  final String? sortKey;
  final bool isImage;
  final double? maxWidth;
}

class RowGuard {
  const RowGuard({required this.isAllowed, required this.reason});

  final bool isAllowed;
  final String reason;

  static const RowGuard allowed = RowGuard(isAllowed: true, reason: '');
}

class RowAction<T> {
  const RowAction({
    required this.tooltip,
    required this.icon,
    required this.onPressed,
    this.guard,
    this.color = AppColors.primaryIndigo,
  });

  final String tooltip;
  final IconData icon;
  final Future<bool> Function(T item) onPressed;
  final RowGuard Function(T item)? guard;
  final Color color;
}

class CrudDataTable<T> extends StatefulWidget {
  const CrudDataTable({
    super.key,
    required this.title,
    required this.columns,
    required this.loadPage,
    this.subtitle,
    this.onCreate,
    this.onEdit,
    this.onDelete,
    this.deleteGuard,
    this.searchHint = 'Search',
    this.createLabel = 'ADD NEW',
    this.imageUrlOf,
    this.imageFit = BoxFit.cover,
    this.filters = const [],
    this.editTooltip = 'Edit',
    this.editIcon = Icons.edit_outlined,
    this.rowActions = const [],
  });

  final String title;
  final String? subtitle;
  final List<CrudColumn<T>> columns;
  final PageLoader<T> loadPage;
  final Future<bool> Function()? onCreate;
  final Future<bool> Function(T item)? onEdit;
  final Future<bool> Function(T item)? onDelete;
  final RowGuard Function(T item)? deleteGuard;
  final String searchHint;
  final String createLabel;
  final String? Function(T item)? imageUrlOf;
  final BoxFit imageFit;
  final List<Widget> filters;
  final String editTooltip;
  final IconData editIcon;
  final List<RowAction<T>> rowActions;

  @override
  State<CrudDataTable<T>> createState() => CrudDataTableState<T>();
}

class CrudDataTableState<T> extends State<CrudDataTable<T>> {
  static const int pageSize = 10;

  final TextEditingController _searchController = TextEditingController();
  final ScrollController _horizontalController = ScrollController();

  List<T> _items = const [];
  int _totalCount = 0;
  int _page = 1;
  String? _sortBy;
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    refresh();
  }

  @override
  void dispose() {
    _searchController.dispose();
    _horizontalController.dispose();
    super.dispose();
  }

  Future<void> refresh({bool toFirstPage = false}) async {
    if (toFirstPage) {
      _page = 1;
    }

    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      final result = await widget.loadPage(
        _page,
        pageSize,
        _searchController.text.trim().isEmpty
            ? null
            : _searchController.text.trim(),
        _sortBy,
      );

      if (!mounted) return;

      setState(() {
        _items = result.items ?? const [];
        _totalCount = result.totalCount ?? 0;
        _isLoading = false;
      });
    } catch (e) {
      if (!mounted) return;

      setState(() {
        _error = e.toString();
        _isLoading = false;
      });
    }
  }

  int get _lastPage =>
      _totalCount == 0 ? 1 : ((_totalCount + pageSize - 1) ~/ pageSize);

  Future<void> _handleCreate() async {
    final created = await widget.onCreate!.call();

    if (created) {
      _sortBy = 'Id desc';
      await refresh(toFirstPage: true);
    }
  }

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        color: AppColors.surface,
        border: Border.all(color: AppColors.border),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          _buildToolbar(context),
          const Divider(height: 1),
          Expanded(child: _buildBody(context)),
          const Divider(height: 1),
          _buildFooter(context),
        ],
      ),
    );
  }

  Widget _buildToolbar(BuildContext context) {
    final theme = Theme.of(context);

    final heading = Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      mainAxisSize: MainAxisSize.min,
      children: [
        Text(widget.title, style: theme.textTheme.titleMedium),
        if (widget.subtitle != null) ...[
          const SizedBox(height: 2),
          Text(widget.subtitle!, style: theme.textTheme.bodySmall),
        ],
      ],
    );

    final search = SizedBox(
      width: AppSpacing.toolbarFieldWidth,
      child: TextField(
        controller: _searchController,
        style: theme.textTheme.bodyMedium,
        decoration: InputDecoration(
          hintText: widget.searchHint,
          prefixIcon: const Icon(Icons.search, size: 18),
          prefixIconConstraints: const BoxConstraints(minWidth: 38),
        ),
        onSubmitted: (_) => refresh(toFirstPage: true),
      ),
    );

    final searchButton = OutlinedButton.icon(
      onPressed: () => refresh(toFirstPage: true),
      icon: const Icon(Icons.filter_alt_outlined, size: 16),
      label: const Text('Search'),
    );

    final sortMenu = _SortMenu<T>(
      columns: widget.columns,
      current: _sortBy,
      onChanged: (value) {
        _sortBy = value;
        refresh(toFirstPage: true);
      },
    );

    return Padding(
      padding: const EdgeInsets.all(AppSpacing.md),
      child: LayoutBuilder(
        builder: (context, constraints) {
          if (constraints.maxWidth < 860) {
            return Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                heading,
                const SizedBox(height: AppSpacing.md),
                Wrap(
                  spacing: AppSpacing.sm,
                  runSpacing: AppSpacing.sm,
                  crossAxisAlignment: WrapCrossAlignment.center,
                  children: [search, ...widget.filters, searchButton, sortMenu],
                ),
              ],
            );
          }

          return Row(
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              Expanded(flex: 2, child: heading),
              const SizedBox(width: AppSpacing.md),
              Expanded(
                flex: 5,
                child: Wrap(
                  alignment: WrapAlignment.end,
                  spacing: AppSpacing.sm,
                  runSpacing: AppSpacing.sm,
                  crossAxisAlignment: WrapCrossAlignment.center,
                  children: [search, ...widget.filters, searchButton, sortMenu],
                ),
              ),
            ],
          );
        },
      ),
    );
  }

  Widget _buildFooter(BuildContext context) {
    final theme = Theme.of(context);

    return Padding(
      padding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.md,
        vertical: AppSpacing.sm,
      ),
      child: Row(
        children: [
          Text(
            _totalCount == 1 ? '1 record' : '$_totalCount records',
            style: theme.textTheme.bodySmall,
          ),
          const Spacer(),
          IconButton(
            tooltip: 'Previous page',
            visualDensity: VisualDensity.compact,
            onPressed: _page > 1
                ? () {
                    _page--;
                    refresh();
                  }
                : null,
            icon: const Icon(Icons.chevron_left, size: 20),
          ),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: AppSpacing.xs),
            child: Text(
              '$_page / $_lastPage',
              style: theme.textTheme.bodySmall,
            ),
          ),
          IconButton(
            tooltip: 'Next page',
            visualDensity: VisualDensity.compact,
            onPressed: _page < _lastPage
                ? () {
                    _page++;
                    refresh();
                  }
                : null,
            icon: const Icon(Icons.chevron_right, size: 20),
          ),
          if (widget.onCreate != null) ...[
            const SizedBox(width: AppSpacing.md),
            FilledButton.icon(
              onPressed: _handleCreate,
              icon: const Icon(Icons.add, size: 18),
              label: Text(widget.createLabel),
            ),
          ],
        ],
      ),
    );
  }

  Widget _buildBody(BuildContext context) {
    final theme = Theme.of(context);

    if (_isLoading) {
      return const Center(
        child: SizedBox(
          height: 28,
          width: 28,
          child: CircularProgressIndicator(strokeWidth: 2.5),
        ),
      );
    }

    if (_error != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.lg),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(
                Icons.error_outline,
                color: theme.colorScheme.error,
                size: 32,
              ),
              const SizedBox(height: AppSpacing.sm),
              Text(
                _error!,
                textAlign: TextAlign.center,
                style: theme.textTheme.bodyMedium,
              ),
              const SizedBox(height: AppSpacing.md),
              OutlinedButton(
                onPressed: () => refresh(),
                child: const Text('Try again'),
              ),
            ],
          ),
        ),
      );
    }

    if (_items.isEmpty) {
      final isSearching = _searchController.text.trim().isNotEmpty;

      return Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.lg),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(
                isSearching ? Icons.search_off : Icons.inbox_outlined,
                size: 32,
                color: AppColors.textMuted,
              ),
              const SizedBox(height: AppSpacing.sm),
              Text(
                isSearching
                    ? 'No matches for "${_searchController.text.trim()}".'
                    : widget.onCreate == null
                    ? 'Nothing to show yet.'
                    : 'Nothing here yet. Use ${widget.createLabel} to create the first one.',
                style: theme.textTheme.bodyMedium,
                textAlign: TextAlign.center,
              ),
            ],
          ),
        ),
      );
    }

    return Scrollbar(
      child: SingleChildScrollView(
        child: LayoutBuilder(
          builder: (context, constraints) {
            final share = _fairColumnWidth(constraints.maxWidth);

            return Scrollbar(
              controller: _horizontalController,
              thumbVisibility: true,
              scrollbarOrientation: ScrollbarOrientation.bottom,
              child: SingleChildScrollView(
                controller: _horizontalController,
                scrollDirection: Axis.horizontal,
                padding: const EdgeInsets.only(bottom: AppSpacing.md),
                child: ConstrainedBox(
                  constraints: BoxConstraints(minWidth: constraints.maxWidth),
                  child: DataTable(
                    showCheckboxColumn: false,
                    columnSpacing: AppSpacing.lg,
                    horizontalMargin: AppSpacing.md,
                    border: const TableBorder(
                      horizontalInside: BorderSide(color: AppColors.border),
                    ),
                    columns: [
                      if (widget.imageUrlOf != null)
                        const DataColumn(
                          label: SizedBox(width: 36),
                          columnWidth: FixedColumnWidth(52),
                        ),
                      for (final column in widget.columns)
                        DataColumn(
                          label: _CellText(
                            column: column,
                            value: column.label,
                            share: share,
                          ),
                          columnWidth: const IntrinsicColumnWidth(flex: 1),
                        ),
                      if (_hasRowActions)
                        const DataColumn(label: SizedBox.shrink()),
                    ],
                    rows: [
                      for (final item in _items)
                        DataRow(
                          color: WidgetStateProperty.resolveWith(
                            (states) => states.contains(WidgetState.hovered)
                                ? AppColors.rowHover
                                : null,
                          ),
                          cells: [
                            if (widget.imageUrlOf != null)
                              DataCell(_thumbnail(widget.imageUrlOf!(item))),
                            for (final column in widget.columns)
                              DataCell(
                                _CellText(
                                  column: column,
                                  value: column.value(item),
                                  share: share,
                                ),
                              ),
                            if (_hasRowActions)
                              DataCell(_buildRowActions(item)),
                          ],
                        ),
                    ],
                  ),
                ),
              ),
            );
          },
        ),
      ),
    );
  }

  double _fairColumnWidth(double available) {
    final reserved =
        (widget.imageUrlOf != null ? 52.0 : 0.0) +
        (_hasRowActions ? 120.0 : 0.0);
    final spacing = AppSpacing.lg * widget.columns.length + AppSpacing.md * 2;
    final usable = available - reserved - spacing;
    final share = usable / widget.columns.length;

    return share < _CellText.readableWidth ? _CellText.readableWidth : share;
  }

  Widget _thumbnail(String? url) =>
      ApiImage.avatar(url: url, fit: widget.imageFit);

  bool get _hasRowActions =>
      widget.onEdit != null ||
      widget.onDelete != null ||
      widget.rowActions.isNotEmpty;

  Widget _buildRowActions(T item) {
    final guard = widget.deleteGuard?.call(item) ?? RowGuard.allowed;

    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        for (final action in widget.rowActions)
          DisabledAction(
            isEnabled: action.guard?.call(item).isAllowed ?? true,
            reason: action.guard?.call(item).reason ?? '',
            child: IconButton(
              tooltip: action.tooltip,
              visualDensity: VisualDensity.compact,
              icon: Icon(action.icon, color: action.color, size: 19),
              onPressed: () async {
                final changed = await action.onPressed(item);
                if (changed) {
                  await refresh();
                }
              },
            ),
          ),
        if (widget.onEdit != null)
          IconButton(
            tooltip: widget.editTooltip,
            visualDensity: VisualDensity.compact,
            icon: Icon(widget.editIcon, color: AppColors.editGreen, size: 19),
            onPressed: () async {
              final changed = await widget.onEdit!.call(item);
              if (changed) {
                await refresh();
              }
            },
          ),
        if (widget.onDelete != null)
          DisabledAction(
            isEnabled: guard.isAllowed,
            reason: guard.reason,
            child: IconButton(
              tooltip: 'Delete',
              visualDensity: VisualDensity.compact,
              icon: const Icon(
                Icons.delete_outline,
                color: AppColors.danger,
                size: 19,
              ),
              onPressed: () async {
                final deleted = await widget.onDelete!.call(item);
                if (deleted) {
                  await refresh();
                }
              },
            ),
          ),
      ],
    );
  }
}

class _CellText<T> extends StatelessWidget {
  const _CellText({required this.column, required this.value, this.share});

  static const double readableWidth = 140;

  final CrudColumn<T> column;
  final String value;
  final double? share;

  @override
  Widget build(BuildContext context) {
    final text = Text(
      value,
      softWrap: true,
      maxLines: 2,
      overflow: TextOverflow.ellipsis,
    );

    final cap = _cap;

    if (cap == null) {
      return text;
    }

    return ConstrainedBox(
      constraints: BoxConstraints(maxWidth: cap),
      child: text,
    );
  }

  double? get _cap {
    if (column.maxWidth == null) {
      return share;
    }

    if (share == null) {
      return column.maxWidth;
    }

    return column.maxWidth! < share! ? column.maxWidth : share;
  }
}

class _SortMenu<T> extends StatelessWidget {
  const _SortMenu({
    required this.columns,
    required this.current,
    required this.onChanged,
  });

  final List<CrudColumn<T>> columns;
  final String? current;
  final ValueChanged<String?> onChanged;

  @override
  Widget build(BuildContext context) {
    final sortable = columns.where((c) => c.sortKey != null).toList();

    if (sortable.isEmpty) {
      return const SizedBox.shrink();
    }

    return PopupMenuButton<String?>(
      tooltip: 'Sort',
      position: PopupMenuPosition.under,
      onSelected: onChanged,
      itemBuilder: (context) => [
        const PopupMenuItem<String?>(value: null, child: Text('Default order')),
        for (final column in sortable) ...[
          PopupMenuItem<String?>(
            value: column.sortKey,
            child: Text('${column.label} ascending'),
          ),
          PopupMenuItem<String?>(
            value: '${column.sortKey} desc',
            child: Text('${column.label} descending'),
          ),
        ],
      ],
      child: OutlinedButton.icon(
        onPressed: null,
        style: OutlinedButton.styleFrom(
          disabledForegroundColor: AppColors.primaryIndigo,
          disabledIconColor: AppColors.primaryIndigo,
        ),
        icon: const Icon(Icons.sort, size: 16),
        label: Text(current == null ? 'Sort' : 'Sorted: $current'),
      ),
    );
  }
}
