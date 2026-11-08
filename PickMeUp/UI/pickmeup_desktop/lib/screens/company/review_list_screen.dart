import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/review.dart';
import '../../models/search_result.dart';
import '../../providers/review_provider.dart';
import '../../providers/taxi_company_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';
import '../../widgets/crud_data_table.dart';
import '../../widgets/filter_dropdown.dart';

class ReviewListScreen extends StatefulWidget {
  const ReviewListScreen({super.key});

  @override
  State<ReviewListScreen> createState() => _ReviewListScreenState();
}

class _ReviewListScreenState extends State<ReviewListScreen> {
  final _provider = ReviewProvider();
  final _companyProvider = TaxiCompanyProvider();
  final _tableKey = GlobalKey<CrudDataTableState<Review>>();

  RatingSummary? _companyAverage;
  List<RatingSummary> _driverAverages = const [];
  bool _isLoadingAverages = true;
  String? _averagesError;
  int? _minRating;

  @override
  void initState() {
    super.initState();
    _loadAverages();
  }

  Future<void> _loadAverages() async {
    setState(() {
      _isLoadingAverages = true;
      _averagesError = null;
    });

    try {
      final company = await _companyProvider.getMine();
      final companies = await _provider.companyAverages();
      final drivers = await _provider.driverAverages();

      if (!mounted) return;

      setState(() {
        _companyAverage = companies
            .where((summary) => summary.name == company.name)
            .cast<RatingSummary?>()
            .firstWhere((summary) => true, orElse: () => null);
        _driverAverages = drivers
            .where((summary) => summary.companyName == company.name)
            .toList();
        _isLoadingAverages = false;
      });
    } on ApiClientException catch (e) {
      if (!mounted) return;
      setState(() {
        _averagesError = e.message;
        _isLoadingAverages = false;
      });
    } catch (_) {
      if (!mounted) return;
      setState(() {
        _averagesError =
            'Could not reach the server. Check that the API is running.';
        _isLoadingAverages = false;
      });
    }
  }

  Future<SearchResult<Review>> _load(
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
      'AuthorName': ?search,
      'MinRating': ?_minRating,
    });
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text('Reviews', style: theme.textTheme.headlineSmall),
        const SizedBox(height: AppSpacing.xs),
        Text(
          'What passengers said about your company and your drivers.',
          style: theme.textTheme.bodySmall,
        ),
        const SizedBox(height: AppSpacing.md),
        _buildAverages(theme),
        const SizedBox(height: AppSpacing.md),
        Expanded(
          child: CrudDataTable<Review>(
            key: _tableKey,
            title: 'Every review',
            searchHint: 'Search by passenger name',
            loadPage: _load,
            filters: [
              FilterDropdown<int>(
                value: _minRating,
                width: 170,
                options: const [
                  FilterOption<int>(label: 'Any rating', value: null),
                  FilterOption<int>(label: '4 stars and up', value: 4),
                  FilterOption<int>(label: '3 stars and up', value: 3),
                  FilterOption<int>(label: '2 stars and up', value: 2),
                ],
                onChanged: (value) {
                  setState(() => _minRating = value);
                  _tableKey.currentState?.refresh(toFirstPage: true);
                },
              ),
            ],
            columns: [
              CrudColumn(
                label: 'Rating',
                value: (r) => '${r.rating ?? 0} / 5',
                sortKey: 'Rating',
              ),
              CrudColumn(
                label: 'About',
                value: (r) => r.target,
                maxWidth: 220,
              ),
              CrudColumn(
                label: 'Passenger',
                value: (r) => r.authorName ?? '',
                maxWidth: 150,
              ),
              CrudColumn(
                label: 'Comment',
                value: (r) => r.comment ?? '',
                maxWidth: 280,
              ),
              CrudColumn(
                label: 'Left on',
                value: (r) => Formats.day(r.createdAt),
                sortKey: 'CreatedAt',
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildAverages(ThemeData theme) {
    if (_isLoadingAverages) {
      return const SizedBox(
        height: 20,
        width: 20,
        child: CircularProgressIndicator(strokeWidth: 2),
      );
    }

    final failure = _averagesError;

    if (failure != null) {
      return Row(
        children: [
          Expanded(
            child: Text(
              failure,
              style: theme.textTheme.bodySmall?.copyWith(color: AppColors.danger),
            ),
          ),
          TextButton(onPressed: _loadAverages, child: const Text('RETRY')),
        ],
      );
    }

    return SingleChildScrollView(
      scrollDirection: Axis.horizontal,
      child: Row(
        children: [
          _AverageCard(
            label: 'Company average',
            summary: _companyAverage,
            isPrimary: true,
          ),
          for (final driver in _driverAverages) ...[
            const SizedBox(width: AppSpacing.sm),
            _AverageCard(label: driver.name ?? '', summary: driver),
          ],
        ],
      ),
    );
  }
}

class _AverageCard extends StatelessWidget {
  const _AverageCard({
    required this.label,
    required this.summary,
    this.isPrimary = false,
  });

  final String label;
  final RatingSummary? summary;
  final bool isPrimary;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      width: 210,
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: isPrimary ? AppColors.rowHover : AppColors.surface,
        border: Border.all(
          color: isPrimary ? AppColors.primaryIndigo : AppColors.border,
        ),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: theme.textTheme.titleSmall),
          const SizedBox(height: AppSpacing.sm),
          Row(
            crossAxisAlignment: CrossAxisAlignment.baseline,
            textBaseline: TextBaseline.alphabetic,
            children: [
              Icon(
                Icons.star_rounded,
                size: 20,
                color: summary?.averageRating == null
                    ? AppColors.textMuted
                    : AppColors.primaryIndigo,
              ),
              const SizedBox(width: AppSpacing.xs),
              Text(
                summary?.averageRating == null
                    ? '—'
                    : summary!.averageRating!.toStringAsFixed(2),
                style: theme.textTheme.headlineSmall,
              ),
            ],
          ),
          const SizedBox(height: 2),
          Text(
            Formats.rating(summary?.averageRating, summary?.reviewCount),
            style: theme.textTheme.bodySmall,
          ),
        ],
      ),
    );
  }
}
