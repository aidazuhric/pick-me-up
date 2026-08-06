import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import 'reference_data_screen.dart';
import 'reference_table.dart';

class ReferenceDataHub extends StatefulWidget {
  const ReferenceDataHub({super.key});

  @override
  State<ReferenceDataHub> createState() => _ReferenceDataHubState();
}

class _ReferenceDataHubState extends State<ReferenceDataHub> {
  int _selectedIndex = 0;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final selected = referenceTables[_selectedIndex];

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text('Reference data', style: theme.textTheme.headlineSmall),
        const SizedBox(height: AppSpacing.xs),
        Text(
          'The lookup lists the rest of the platform chooses from.',
          style: theme.textTheme.bodySmall,
        ),
        const SizedBox(height: AppSpacing.md),
        SingleChildScrollView(
          scrollDirection: Axis.horizontal,
          child: Row(
            children: [
              for (var index = 0; index < referenceTables.length; index++)
                Padding(
                  padding: const EdgeInsets.only(right: AppSpacing.sm),
                  child: _TableChip(
                    table: referenceTables[index],
                    isSelected: index == _selectedIndex,
                    onSelected: () => setState(() => _selectedIndex = index),
                  ),
                ),
            ],
          ),
        ),
        const SizedBox(height: AppSpacing.md),
        Expanded(
          child: ReferenceDataScreen(
            key: ValueKey(selected.endpoint),
            table: selected,
          ),
        ),
      ],
    );
  }
}

class _TableChip extends StatelessWidget {
  const _TableChip({
    required this.table,
    required this.isSelected,
    required this.onSelected,
  });

  final ReferenceTable table;
  final bool isSelected;
  final VoidCallback onSelected;

  @override
  Widget build(BuildContext context) {
    return ChoiceChip(
      selected: isSelected,
      onSelected: (_) => onSelected(),
      avatar: Icon(
        table.icon,
        size: 18,
        color: isSelected ? Colors.white : AppColors.primaryIndigo,
      ),
      label: Text(table.title),
      labelStyle: TextStyle(
        fontSize: 13,
        color: isSelected ? Colors.white : AppColors.textPrimary,
        fontWeight: isSelected ? FontWeight.w600 : FontWeight.w500,
      ),
      selectedColor: AppColors.primaryIndigo,
      backgroundColor: Colors.white,
      showCheckmark: false,
      side: BorderSide(
        color: isSelected ? AppColors.primaryIndigo : AppColors.divider,
      ),
      padding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.sm,
        vertical: AppSpacing.sm,
      ),
    );
  }
}
