import 'package:flutter/material.dart';

import '../../models/reference_item.dart';
import '../../models/search_result.dart';
import '../../providers/reference_provider.dart';
import '../../utils/api_error_handler.dart';
import '../../widgets/app_messages.dart';
import '../../widgets/confirm_dialog.dart';
import '../../widgets/crud_data_table.dart';
import 'reference_form.dart';
import 'reference_table.dart';

class ReferenceDataScreen extends StatefulWidget {
  const ReferenceDataScreen({super.key, required this.table});

  final ReferenceTable table;

  @override
  State<ReferenceDataScreen> createState() => _ReferenceDataScreenState();
}

class _ReferenceDataScreenState extends State<ReferenceDataScreen> {
  late final ReferenceProvider _provider =
      ReferenceProvider(widget.table.endpoint);

  Future<SearchResult<ReferenceItem>> _load(
    int page,
    int pageSize,
    String? search,
    String? sortBy,
  ) {
    return _provider.get(filter: {
      'Page': page,
      'PageSize': pageSize,
      'IncludeTotalCount': true,
      'SortBy': ?sortBy,
      'Name': ?search,
    });
  }

  Future<bool> _create() {
    return ReferenceForm.show(
      context,
      table: widget.table,
      provider: _provider,
    );
  }

  Future<bool> _edit(ReferenceItem item) {
    return ReferenceForm.show(
      context,
      table: widget.table,
      provider: _provider,
      existing: item,
    );
  }

  Future<bool> _delete(ReferenceItem item) async {
    final confirmed = await ConfirmDialog.show(
      context,
      title: widget.table.deleteQuestion,
      message: '${item.name} — ${widget.table.deleteWarning}',
      confirmLabel: 'DELETE',
      isDestructive: true,
    );

    if (!confirmed) {
      return false;
    }

    try {
      await _provider.remove(item.id!);
      if (!mounted) return false;
      AppMessages.success(context, '${item.name} was removed.');
      return true;
    } catch (e) {
      if (!mounted) return false;
      ApiErrorHandler.surface(context, e);
      return false;
    }
  }

  @override
  Widget build(BuildContext context) {
    final table = widget.table;

    return CrudDataTable<ReferenceItem>(
      title: table.title,
      searchHint: table.searchHint,
      loadPage: _load,
      onCreate: _create,
      onEdit: _edit,
      onDelete: _delete,
      columns: [
        CrudColumn(
          label: table.nameLabel,
          value: (item) => item.name ?? '',
          sortKey: 'Name',
          maxWidth: 220,
        ),
        if (table.hasPostalCode)
          CrudColumn(
            label: 'Postal code',
            value: (item) => item.postalCode ?? '',
          ),
        if (table.hasDescription)
          CrudColumn(
            label: 'Description',
            value: (item) => item.description ?? '',
            maxWidth: 280,
          ),
        if (table.hasSortOrder)
          CrudColumn(
            label: 'Display order',
            value: (item) => item.sortOrder?.toString() ?? '',
            sortKey: 'SortOrder',
          ),
        CrudColumn(
          label: 'Available',
          value: (item) => item.isActive == true ? 'Yes' : 'No',
          sortKey: 'IsActive',
        ),
      ],
    );
  }
}
