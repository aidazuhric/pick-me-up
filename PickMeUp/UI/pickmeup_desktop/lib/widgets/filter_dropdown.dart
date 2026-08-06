import 'package:flutter/material.dart';

import '../core/theme/app_spacing.dart';

class FilterOption<T> {
  const FilterOption({required this.label, required this.value});

  final String label;
  final T? value;
}

class FilterDropdown<T> extends StatelessWidget {
  const FilterDropdown({
    super.key,
    required this.options,
    required this.value,
    required this.onChanged,
    this.width = 180,
  });

  final List<FilterOption<T>> options;
  final T? value;
  final ValueChanged<T?> onChanged;
  final double width;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: width,
      child: DropdownButtonFormField<T?>(
        initialValue: value,
        isDense: true,
        isExpanded: true,
        style: Theme.of(context).textTheme.bodyMedium,
        icon: const Icon(Icons.expand_more, size: 20),
        decoration: const InputDecoration(
          contentPadding: EdgeInsets.symmetric(
            horizontal: AppSpacing.sm + AppSpacing.xs,
            vertical: 12,
          ),
        ),
        items: [
          for (final option in options)
            DropdownMenuItem<T?>(
              value: option.value,
              child: Text(option.label),
            ),
        ],
        onChanged: onChanged,
      ),
    );
  }
}
