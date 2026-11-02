import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../models/search_result.dart';
import '../../models/user.dart';
import '../../providers/auth_provider.dart';
import '../../providers/user_provider.dart';
import '../../utils/api_error_handler.dart';
import '../../widgets/app_messages.dart';
import '../../widgets/confirm_dialog.dart';
import '../../widgets/crud_data_table.dart';
import '../../widgets/filter_dropdown.dart';
import 'user_form.dart';

class UserListScreen extends StatefulWidget {
  const UserListScreen({super.key});

  @override
  State<UserListScreen> createState() => _UserListScreenState();
}

class _UserListScreenState extends State<UserListScreen> {
  final _provider = UserProvider();
  final _tableKey = GlobalKey<CrudDataTableState<User>>();

  bool? _isActive;
  String? _role;

  Future<SearchResult<User>> _load(
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
      'IsActive': ?_isActive,
      'Role': ?_role,
    });
  }

  Future<bool> _edit(User user) {
    final signedInId = context.read<AuthProvider>().userId;

    return UserForm.show(
      context,
      user: user,
      provider: _provider,
      isSelf: signedInId != null && signedInId == user.id,
    );
  }

  Future<bool> _delete(User user) async {
    final confirmed = await ConfirmDialog.show(
      context,
      title: 'Remove ${user.fullName}?',
      message:
          'The account stops appearing and can no longer sign in. Their rides, payments and tickets are kept for the record.',
      confirmLabel: 'DELETE',
      isDestructive: true,
    );

    if (!confirmed) {
      return false;
    }

    try {
      await _provider.remove(user.id!);
      if (!mounted) return false;
      AppMessages.success(context, '${user.fullName} was removed.');
      return true;
    } catch (e) {
      if (!mounted) return false;
      ApiErrorHandler.surface(context, e);
      return false;
    }
  }

  RowGuard _deleteGuard(User user) {
    final signedInId = context.read<AuthProvider>().userId;

    if (signedInId != null && signedInId == user.id) {
      return const RowGuard(
        isAllowed: false,
        reason: 'You cannot remove the account you are signed in with.',
      );
    }

    return RowGuard.allowed;
  }

  @override
  Widget build(BuildContext context) {
    return CrudDataTable<User>(
      key: _tableKey,
      title: 'Users',
      subtitle: 'Every account on the platform, across all four roles.',
      searchHint: 'Search by first or last name',
      loadPage: _load,
      onEdit: _edit,
      onDelete: _delete,
      deleteGuard: _deleteGuard,
      imageUrlOf: (user) => user.profileImageUrl,
      filters: [
        FilterDropdown<bool>(
          value: _isActive,
          width: 170,
          options: const [
            FilterOption<bool>(label: 'Any status', value: null),
            FilterOption<bool>(label: 'Active only', value: true),
            FilterOption<bool>(label: 'Suspended only', value: false),
          ],
          onChanged: (value) {
            setState(() => _isActive = value);
            _tableKey.currentState?.refresh(toFirstPage: true);
          },
        ),
        FilterDropdown<String>(
          value: _role,
          width: 170,
          options: const [
            FilterOption<String>(label: 'Any role', value: null),
            FilterOption<String>(label: 'Admin', value: 'Admin'),
            FilterOption<String>(label: 'Company', value: 'Company'),
            FilterOption<String>(label: 'Driver', value: 'Driver'),
            FilterOption<String>(label: 'Passenger', value: 'Passenger'),
          ],
          onChanged: (value) {
            setState(() => _role = value);
            _tableKey.currentState?.refresh(toFirstPage: true);
          },
        ),
      ],
      columns: [
        CrudColumn(
          label: 'Name',
          value: (user) => user.fullName,
          sortKey: 'FirstName',
          maxWidth: 200,
        ),
        CrudColumn(
          label: 'Username',
          value: (user) => user.username ?? '',
          sortKey: 'Username',
        ),
        CrudColumn(
          label: 'Email',
          value: (user) => user.email ?? '',
          sortKey: 'Email',
          maxWidth: 240,
        ),
        CrudColumn(label: 'Role', value: (user) => user.role ?? ''),
        CrudColumn(
          label: 'Phone',
          value: (user) => user.phoneNumber ?? '',
        ),
        CrudColumn(
          label: 'Status',
          value: (user) => user.isActive == true ? 'Active' : 'Suspended',
          sortKey: 'IsActive',
        ),
      ],
    );
  }
}
