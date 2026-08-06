import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/taxi_company.dart';
import '../../providers/car_provider.dart';
import '../../providers/driver_provider.dart';
import '../../providers/taxi_company_provider.dart';
import '../../utils/api_error_handler.dart';

class CompanyProfileScreen extends StatefulWidget {
  const CompanyProfileScreen({super.key});

  @override
  State<CompanyProfileScreen> createState() => _CompanyProfileScreenState();
}

class _CompanyProfileScreenState extends State<CompanyProfileScreen> {
  final _companyProvider = TaxiCompanyProvider();
  final _carProvider = CarProvider();
  final _driverProvider = DriverProvider();

  TaxiCompany? _company;
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      final name = await _resolveOwnCompanyName();

      if (name == null) {
        if (!mounted) return;
        setState(() {
          _company = null;
          _isLoading = false;
        });
        return;
      }

      final result = await _companyProvider.get(filter: {'Name': name, 'PageSize': 1});

      if (!mounted) return;
      setState(() {
        _company = (result.items ?? const []).isEmpty ? null : result.items!.first;
        _isLoading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e.toString();
        _isLoading = false;
      });
      ApiErrorHandler.surface(context, e);
    }
  }

  Future<String?> _resolveOwnCompanyName() async {
    final cars = await _carProvider.get(filter: {'PageSize': 1});
    final fromCars = (cars.items ?? const []);
    if (fromCars.isNotEmpty && fromCars.first.companyName != null) {
      return fromCars.first.companyName;
    }

    final drivers = await _driverProvider.get(filter: {'PageSize': 1});
    final fromDrivers = (drivers.items ?? const []);
    if (fromDrivers.isNotEmpty && fromDrivers.first.companyName != null) {
      return fromDrivers.first.companyName;
    }

    return null;
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text('Company profile', style: theme.textTheme.headlineSmall),
        const SizedBox(height: AppSpacing.xs),
        Text(
          'How your company appears to passengers, and the pricing the fare engine uses.',
          style: theme.textTheme.bodySmall,
        ),
        const SizedBox(height: AppSpacing.lg),
        Expanded(child: _buildBody(theme)),
      ],
    );
  }

  Widget _buildBody(ThemeData theme) {
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
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(_error!, style: theme.textTheme.bodyMedium),
            const SizedBox(height: AppSpacing.md),
            OutlinedButton(onPressed: _load, child: const Text('Try again')),
          ],
        ),
      );
    }

    final company = _company;

    if (company == null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.lg),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.badge_outlined, size: 32, color: AppColors.textMuted),
              const SizedBox(height: AppSpacing.sm),
              Text(
                'Your profile could not be identified yet. It appears once your company has at least one vehicle or driver.',
                style: theme.textTheme.bodyMedium,
                textAlign: TextAlign.center,
              ),
            ],
          ),
        ),
      );
    }

    return SingleChildScrollView(
      child: Align(
        alignment: Alignment.topLeft,
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: AppSpacing.formMaxWidth),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              _Card(
                title: company.name ?? '',
                subtitle: company.isActive == true
                    ? 'Visible to passengers'
                    : 'Hidden from passengers',
                children: [
                  _Row(label: 'Headquarters', value: company.hqAddress ?? ''),
                  _Row(label: 'City', value: company.cityName ?? ''),
                  _Row(label: 'Owner', value: company.ownerName ?? ''),
                ],
              ),
              const SizedBox(height: AppSpacing.md),
              _Card(
                title: 'Contact',
                children: [
                  _Row(label: 'Email', value: company.contactEmail ?? ''),
                  _Row(label: 'Phone', value: company.contactPhone ?? ''),
                ],
              ),
              const SizedBox(height: AppSpacing.md),
              _Card(
                title: 'Pricing the fare engine uses',
                children: [
                  _Row(
                    label: 'Starting price',
                    value: '${company.startingPrice?.toStringAsFixed(2)} BAM',
                  ),
                  _Row(
                    label: 'Price per kilometre',
                    value: '${company.pricePerKm?.toStringAsFixed(2)} BAM',
                  ),
                ],
              ),
              const SizedBox(height: AppSpacing.md),
              _Notice(
                text:
                    'Changes to the profile and the pricing are made by an administrator. A company cannot delete itself.',
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _Card extends StatelessWidget {
  const _Card({required this.title, required this.children, this.subtitle});

  final String title;
  final String? subtitle;
  final List<Widget> children;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return DecoratedBox(
      decoration: BoxDecoration(
        color: AppColors.surface,
        border: Border.all(color: AppColors.border),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Padding(
            padding: const EdgeInsets.all(AppSpacing.md),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(title, style: theme.textTheme.titleMedium),
                if (subtitle != null) ...[
                  const SizedBox(height: 2),
                  Text(subtitle!, style: theme.textTheme.bodySmall),
                ],
              ],
            ),
          ),
          const Divider(height: 1),
          for (var i = 0; i < children.length; i++) ...[
            if (i > 0) const Divider(height: 1),
            Padding(
              padding: const EdgeInsets.all(AppSpacing.md),
              child: children[i],
            ),
          ],
        ],
      ),
    );
  }
}

class _Row extends StatelessWidget {
  const _Row({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Row(
      children: [
        SizedBox(
          width: AppSpacing.formLabelWidth,
          child: Text(label, style: theme.textTheme.bodyMedium),
        ),
        Expanded(
          child: Text(
            value,
            style: theme.textTheme.bodyMedium
                ?.copyWith(fontWeight: FontWeight.w600),
          ),
        ),
      ],
    );
  }
}

class _Notice extends StatelessWidget {
  const _Notice({required this.text});

  final String text;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.sectionTint,
        border: Border.all(color: AppColors.border),
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
      ),
      child: Row(
        children: [
          const Icon(Icons.info_outline, size: 18, color: AppColors.textMuted),
          const SizedBox(width: AppSpacing.sm),
          Expanded(
            child: Text(text, style: Theme.of(context).textTheme.bodySmall),
          ),
        ],
      ),
    );
  }
}
