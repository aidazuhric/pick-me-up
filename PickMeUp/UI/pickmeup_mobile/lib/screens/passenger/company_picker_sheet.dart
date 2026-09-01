import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/taxi_company.dart';
import '../../providers/taxi_company_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';

class CompanyChoice {
  const CompanyChoice(this.company);

  final TaxiCompany? company;

  bool get isGeneral => company == null;
}

class CompanyPickerSheet extends StatefulWidget {
  const CompanyPickerSheet({super.key, required this.selectedId, this.cityId});

  final int? selectedId;
  final int? cityId;

  static Future<CompanyChoice?> show(
    BuildContext context, {
    int? selectedId,
    int? cityId,
  }) {
    return showModalBottomSheet<CompanyChoice>(
      context: context,
      isScrollControlled: true,
      backgroundColor: AppColors.surface,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(
          top: Radius.circular(AppSpacing.cardRadius),
        ),
      ),
      builder: (_) => CompanyPickerSheet(selectedId: selectedId, cityId: cityId),
    );
  }

  @override
  State<CompanyPickerSheet> createState() => _CompanyPickerSheetState();
}

class _CompanyPickerSheetState extends State<CompanyPickerSheet> {
  final _provider = TaxiCompanyProvider();

  List<TaxiCompany> _companies = const [];
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final result = await _provider.get(
        filter: {
          'IsActive': true,
          'Page': 1,
          'PageSize': 50,
          'SortBy': 'Name',
          if (widget.cityId != null) 'CityId': widget.cityId,
        },
      );

      if (!mounted) return;

      setState(() {
        _companies = result.items ?? const <TaxiCompany>[];
        _isLoading = false;
      });
    } on ApiClientException catch (e) {
      if (!mounted) return;

      setState(() {
        _error = e.message;
        _isLoading = false;
      });
    } catch (_) {
      if (!mounted) return;

      setState(() {
        _error = 'Could not load the companies. Check that the API is running.';
        _isLoading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return SafeArea(
      top: false,
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.screenPadding),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text('Who should drive?', style: theme.textTheme.titleMedium),
                ),
                IconButton(
                  tooltip: 'Close',
                  icon: const Icon(Icons.close),
                  onPressed: () => Navigator.of(context).pop(),
                ),
              ],
            ),
            const SizedBox(height: AppSpacing.sm),
            ListTile(
              contentPadding: EdgeInsets.zero,
              leading: const Icon(Icons.groups_outlined),
              title: const Text('Any available driver'),
              subtitle: const Text(
                'A general order every company can pick up. Usually the fastest.',
              ),
              trailing: widget.selectedId == null
                  ? const Icon(Icons.check_circle, color: AppColors.toggleOn)
                  : null,
              onTap: () =>
                  Navigator.of(context).pop(const CompanyChoice(null)),
            ),
            const Divider(height: 1),
            const SizedBox(height: AppSpacing.sm),
            ConstrainedBox(
              constraints: BoxConstraints(
                maxHeight: MediaQuery.of(context).size.height * 0.42,
              ),
              child: _buildCompanies(theme),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildCompanies(ThemeData theme) {
    if (_isLoading) {
      return const Padding(
        padding: EdgeInsets.symmetric(vertical: AppSpacing.lg),
        child: Center(
          child: SizedBox(
            height: 26,
            width: 26,
            child: CircularProgressIndicator(strokeWidth: 2.5),
          ),
        ),
      );
    }

    if (_error != null) {
      return Padding(
        padding: const EdgeInsets.symmetric(vertical: AppSpacing.md),
        child: Text(
          _error!,
          style: TextStyle(color: theme.colorScheme.error, fontSize: 13),
        ),
      );
    }

    if (_companies.isEmpty) {
      return Padding(
        padding: const EdgeInsets.symmetric(vertical: AppSpacing.md),
        child: Text(
          'No companies are listed yet. Send a general order instead.',
          style: theme.textTheme.bodySmall,
        ),
      );
    }

    return ListView.separated(
      shrinkWrap: true,
      itemCount: _companies.length,
      separatorBuilder: (_, _) => const Divider(height: 1),
      itemBuilder: (context, index) {
        final company = _companies[index];
        final isSelected = company.id == widget.selectedId;

        return ListTile(
          contentPadding: EdgeInsets.zero,
          leading: const CircleAvatar(
            backgroundColor: AppColors.sectionTint,
            child: Icon(Icons.local_taxi, color: AppColors.mobileHeader),
          ),
          title: Text(company.name ?? ''),
          subtitle: Text(
            '${Formats.money(company.startingPrice)} start · '
            '${Formats.money(company.pricePerKm)} per km',
          ),
          trailing: isSelected
              ? const Icon(Icons.check_circle, color: AppColors.toggleOn)
              : null,
          onTap: () => Navigator.of(context).pop(CompanyChoice(company)),
        );
      },
    );
  }
}
