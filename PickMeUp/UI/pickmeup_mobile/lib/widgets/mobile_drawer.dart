import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../core/theme/app_colors.dart';
import '../core/theme/app_spacing.dart';
import '../layouts/nav_destination.dart';
import '../providers/auth_provider.dart';

class MobileDrawer extends StatelessWidget {
  const MobileDrawer({
    super.key,
    required this.destinations,
    required this.selectedIndex,
    required this.onSelectedIndexChanged,
    required this.roleLabel,
  });

  final List<MobileDestination> destinations;
  final int selectedIndex;
  final ValueChanged<int> onSelectedIndexChanged;
  final String roleLabel;

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();

    return Drawer(
      child: Column(
        children: [
          _DrawerHeader(
            name: auth.displayName ?? auth.role ?? 'PickMeUp',
            roleLabel: roleLabel,
          ),
          Expanded(
            child: ListView.builder(
              padding: const EdgeInsets.symmetric(vertical: AppSpacing.sm),
              itemCount: destinations.length,
              itemBuilder: (context, index) {
                final destination = destinations[index];
                final isSelected = index == selectedIndex;

                return ListTile(
                  leading: Icon(
                    destination.icon,
                    color: isSelected
                        ? AppColors.accentCyan
                        : AppColors.textMuted,
                  ),
                  title: Text(
                    destination.label,
                    style: TextStyle(
                      fontSize: 15,
                      fontWeight: isSelected
                          ? FontWeight.w600
                          : FontWeight.w400,
                      color: isSelected
                          ? AppColors.accentCyan
                          : AppColors.textPrimary,
                    ),
                  ),
                  selected: isSelected,
                  selectedTileColor: AppColors.sectionTint,
                  onTap: () {
                    Navigator.of(context).pop();
                    onSelectedIndexChanged(index);
                  },
                );
              },
            ),
          ),
          const Divider(),
          ListTile(
            leading: const Icon(Icons.logout, color: AppColors.danger),
            title: const Text(
              'Log out',
              style: TextStyle(
                fontSize: 15,
                color: AppColors.danger,
                fontWeight: FontWeight.w500,
              ),
            ),
            onTap: () => _confirmLogout(context),
          ),
          const SizedBox(height: AppSpacing.sm),
        ],
      ),
    );
  }

  Future<void> _confirmLogout(BuildContext context) async {
    final navigator = Navigator.of(context);
    final auth = context.read<AuthProvider>();

    final confirmed = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Log out'),
        content: const Text('Do you want to sign out of PickMeUp?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(false),
            child: const Text('CANCEL'),
          ),
          FilledButton(
            onPressed: () => Navigator.of(dialogContext).pop(true),
            child: const Text('LOG OUT'),
          ),
        ],
      ),
    );

    if (confirmed != true) {
      return;
    }

    navigator.pop();
    await auth.logout();
  }
}

class _DrawerHeader extends StatelessWidget {
  const _DrawerHeader({required this.name, required this.roleLabel});

  final String name;
  final String roleLabel;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      height: AppSpacing.drawerHeaderHeight,
      color: AppColors.mobileHeader,
      padding: const EdgeInsets.fromLTRB(
        AppSpacing.screenPadding,
        AppSpacing.xl,
        AppSpacing.screenPadding,
        AppSpacing.md,
      ),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.end,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const CircleAvatar(
            radius: 28,
            backgroundColor: Colors.white24,
            child: Icon(Icons.person, color: Colors.white, size: 30),
          ),
          const SizedBox(height: AppSpacing.sm),
          Text(
            name,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(
              color: Colors.white,
              fontSize: 17,
              fontWeight: FontWeight.w600,
            ),
          ),
          Text(
            roleLabel,
            style: const TextStyle(color: Colors.white70, fontSize: 13),
          ),
        ],
      ),
    );
  }
}
