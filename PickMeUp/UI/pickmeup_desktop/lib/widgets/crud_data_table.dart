import 'package:flutter/material.dart';

import '../core/theme/app_colors.dart';
import '../models/search_result.dart';
import 'disabled_action.dart';

typedef PageLoader<T> = Future<SearchResult<T>> Function(
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
  });

  final String label;
  final String Function(T item) value;
  final String? sortKey;
  final bool isImage;
}

class RowGuard {
  const RowGuard({required this.isAllowed, required this.reason});

  final bool isAllowed;
  final String reason;

  static const RowGuard allowed = RowGuard(isAllowed: true, reason: '');
}

class CrudDataTable<T> extends StatefulWidget {
  const CrudDataTable({
    super.key,
    required this.title,
    required this.columns,
    required this.loadPage,
    this.onCreate,
    this.onEdit,
    this.onDelete,
    this.deleteGuard,
    this.searchHint = 'Search',
    this.imageUrlOf,
  });

  final String title;
  final List<CrudColumn<T>> columns;
  final PageLoader<T> loadPage;
  final Future<bool> Function()? onCreate;
  final Future<bool> Function(T item)? onEdit;
  final Future<bool> Function(T item)? onDelete;
  final RowGuard Function(T item)? deleteGuard;
  final String searchHint;
  final String? Function(T item)? imageUrlOf;

  @override
  State<CrudDataTable<T>> createState() => CrudDataTableState<T>();
}

class CrudDataTableState<T> extends State<CrudDataTable<T>> {
  static const int pageSize = 10;

  final TextEditingController _searchController = TextEditingController();

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
        _searchController.text.trim().isEmpty ? null : _searchController.text.trim(),
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

  int get _lastPage => _totalCount == 0 ? 1 : ((_totalCount + pageSize - 1) ~/ pageSize);

  Future<void> _handleCreate() async {
    final created = await widget.onCreate!.call();

    if (created) {
      _sortBy = 'Id desc';
      await refresh(toFirstPage: true);
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        LayoutBuilder(
          builder: (context, constraints) {
            final controls = [
              SizedBox(
                width: constraints.maxWidth < 700 ? 200 : 260,
                child: TextField(
                  controller: _searchController,
                  decoration: InputDecoration(
                    hintText: widget.searchHint,
                    prefixIcon: const Icon(Icons.search),
                    isDense: true,
                  ),
                  onSubmitted: (_) => refresh(toFirstPage: true),
                ),
              ),
              OutlinedButton.icon(
                onPressed: () => refresh(toFirstPage: true),
                icon: const Icon(Icons.filter_alt_outlined, size: 18),
                label: const Text('Search'),
              ),
              _SortMenu<T>(
                columns: widget.columns,
                current: _sortBy,
                onChanged: (value) {
                  _sortBy = value;
                  refresh(toFirstPage: true);
                },
              ),
            ];

            final title = Text(widget.title, style: theme.textTheme.headlineSmall);

            if (constraints.maxWidth < 900) {
              return Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  title,
                  const SizedBox(height: 12),
                  Wrap(spacing: 12, runSpacing: 12, children: controls),
                ],
              );
            }

            return Row(
              children: [
                Flexible(child: title),
                const Spacer(),
                for (final control in controls) ...[control, const SizedBox(width: 12)],
              ],
            );
          },
        ),
        const SizedBox(height: 16),
        Expanded(child: _buildBody(theme)),
        const SizedBox(height: 12),
        Row(
          children: [
            Text('$_totalCount total', style: theme.textTheme.bodySmall),
            const Spacer(),
            IconButton(
              tooltip: 'Previous page',
              onPressed: _page > 1
                  ? () {
                      _page--;
                      refresh();
                    }
                  : null,
              icon: const Icon(Icons.chevron_left),
            ),
            Text('$_page / $_lastPage'),
            IconButton(
              tooltip: 'Next page',
              onPressed: _page < _lastPage
                  ? () {
                      _page++;
                      refresh();
                    }
                  : null,
              icon: const Icon(Icons.chevron_right),
            ),
            const SizedBox(width: 16),
            if (widget.onCreate != null)
              FilledButton.icon(
                onPressed: _handleCreate,
                icon: const Icon(Icons.add),
                label: const Text('ADD NEW'),
              ),
          ],
        ),
      ],
    );
  }

  Widget _buildBody(ThemeData theme) {
    if (_isLoading) {
      return const Center(child: CircularProgressIndicator());
    }

    if (_error != null) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.error_outline, color: theme.colorScheme.error, size: 40),
            const SizedBox(height: 12),
            Text(_error!, textAlign: TextAlign.center),
            const SizedBox(height: 12),
            OutlinedButton(onPressed: () => refresh(), child: const Text('Try again')),
          ],
        ),
      );
    }

    if (_items.isEmpty) {
      return Center(
        child: Text(
          _searchController.text.trim().isEmpty
              ? 'Nothing here yet. Use ADD NEW to create the first one.'
              : 'No matches for "${_searchController.text.trim()}".',
          style: theme.textTheme.bodyMedium,
        ),
      );
    }

    return SingleChildScrollView(
      scrollDirection: Axis.horizontal,
      child: SingleChildScrollView(
        child: DataTable(
          headingRowColor: WidgetStatePropertyAll(AppColors.primaryIndigo),
          headingTextStyle: const TextStyle(color: Colors.white, fontWeight: FontWeight.w600),
          columns: [
            if (widget.imageUrlOf != null) const DataColumn(label: Text('')),
            for (final column in widget.columns) DataColumn(label: Text(column.label)),
            const DataColumn(label: Text('')),
          ],
          rows: [
            for (final item in _items)
              DataRow(
                cells: [
                  if (widget.imageUrlOf != null) DataCell(_thumbnail(widget.imageUrlOf!(item))),
                  for (final column in widget.columns) DataCell(Text(column.value(item))),
                  DataCell(_rowActions(item)),
                ],
              ),
          ],
        ),
      ),
    );
  }

  Widget _thumbnail(String? url) {
    if (url == null || url.isEmpty) {
      return const CircleAvatar(
        radius: 16,
        backgroundColor: AppColors.inputFill,
        child: Icon(Icons.image_not_supported_outlined, size: 16),
      );
    }

    return CircleAvatar(radius: 16, backgroundImage: NetworkImage(url));
  }

  Widget _rowActions(T item) {
    final guard = widget.deleteGuard?.call(item) ?? RowGuard.allowed;

    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        if (widget.onEdit != null)
          IconButton(
            tooltip: 'Edit',
            icon: const Icon(Icons.edit, color: AppColors.editGreen),
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
              icon: const Icon(Icons.delete_outline, color: AppColors.danger),
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

class _SortMenu<T> extends StatelessWidget {
  const _SortMenu({required this.columns, required this.current, required this.onChanged});

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
        icon: const Icon(Icons.sort, size: 18),
        label: Text(current == null ? 'Sort' : 'Sort: $current'),
      ),
    );
  }
}
