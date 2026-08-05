import 'package:flutter/material.dart';

import 'app_messages.dart';

typedef OptionLoader<T> = Future<List<T>> Function();

class DbDropdown<T> extends StatefulWidget {
  const DbDropdown({
    super.key,
    required this.loadOptions,
    required this.labelOf,
    required this.idOf,
    required this.value,
    required this.onChanged,
    this.hintText = 'Choose one',
    this.emptyMessage = 'There is nothing to choose from yet.',
    this.onCreateNew,
    this.validator,
  });

  final OptionLoader<T> loadOptions;
  final String Function(T option) labelOf;
  final int Function(T option) idOf;
  final int? value;
  final ValueChanged<int?> onChanged;
  final String hintText;
  final String emptyMessage;
  final Future<bool> Function()? onCreateNew;
  final String? Function(int?)? validator;

  @override
  State<DbDropdown<T>> createState() => DbDropdownState<T>();
}

class DbDropdownState<T> extends State<DbDropdown<T>> {
  List<T> _options = const [];
  bool _isLoading = true;
  String? _error;

  bool get hasOptions => _options.isNotEmpty;

  @override
  void initState() {
    super.initState();
    reload();
  }

  Future<void> reload() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      final options = await widget.loadOptions();
      if (!mounted) return;
      setState(() {
        _options = options;
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

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    if (_isLoading) {
      return const Padding(
        padding: EdgeInsets.symmetric(vertical: 14),
        child: SizedBox(height: 20, width: 20, child: CircularProgressIndicator(strokeWidth: 2)),
      );
    }

    if (_error != null) {
      return Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(_error!, style: TextStyle(color: theme.colorScheme.error)),
          TextButton(onPressed: reload, child: const Text('Try again')),
        ],
      );
    }

    if (_options.isEmpty) {
      return Row(
        children: [
          Expanded(
            child: Text(widget.emptyMessage, style: TextStyle(color: theme.colorScheme.error)),
          ),
          if (widget.onCreateNew != null)
            TextButton.icon(
              onPressed: _createNew,
              icon: const Icon(Icons.add, size: 18),
              label: const Text('Add one'),
            ),
        ],
      );
    }

    return Row(
      children: [
        Expanded(
          child: DropdownButtonFormField<int>(
            initialValue: widget.value,
            decoration: InputDecoration(hintText: widget.hintText),
            items: [
              for (final option in _options)
                DropdownMenuItem<int>(
                  value: widget.idOf(option),
                  child: Text(widget.labelOf(option)),
                ),
            ],
            onChanged: widget.onChanged,
            validator: widget.validator,
            autovalidateMode: AutovalidateMode.onUserInteraction,
          ),
        ),
        if (widget.onCreateNew != null)
          IconButton(
            tooltip: 'Add a new one',
            onPressed: _createNew,
            icon: const Icon(Icons.add_circle_outline),
          ),
      ],
    );
  }

  Future<void> _createNew() async {
    final created = await widget.onCreateNew!.call();

    if (created) {
      await reload();
      if (mounted) {
        AppMessages.success(context, 'Added. Pick it from the list.');
      }
    }
  }
}
