import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/rating_summary.dart';
import '../../models/taxi_company.dart';
import '../../providers/review_provider.dart';
import '../../providers/taxi_company_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';
import '../../widgets/api_image.dart';
import '../../widgets/rating_stars.dart';
import 'company_detail_screen.dart';

class CompaniesScreen extends StatefulWidget {
  const CompaniesScreen({super.key});

  @override
  State<CompaniesScreen> createState() => _CompaniesScreenState();
}

class _CompaniesScreenState extends State<CompaniesScreen> {
  static const int _pageSize = 10;

  final _companies = TaxiCompanyProvider();
  final _reviews = ReviewProvider();
  final _searchController = TextEditingController();

  List<TaxiCompany> _items = const [];
  Map<int, RatingSummary> _ratings = const {};
  int _page = 1;
  int _totalCount = 0;
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });

    final name = _searchController.text.trim();

    try {
      final result = await _companies.get(
        filter: {
          'IsActive': true,
          'Page': _page,
          'PageSize': _pageSize,
          'SortBy': 'Name',
          'IncludeTotalCount': true,
          if (name.isNotEmpty) 'Name': name,
        },
      );

      final averages = await _reviews.readCompanyAverages();

      if (!mounted) return;

      setState(() {
        _items = result.items ?? const <TaxiCompany>[];
        _totalCount = result.totalCount ?? 0;
        _ratings = {
          for (final a in averages)
            if (a.id != null) a.id!: a,
        };
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
        _error = 'Could not reach the server. Check that the API is running.';
        _isLoading = false;
      });
    }
  }

  void _search() {
    setState(() => _page = 1);
    _load();
  }

  void _goToPage(int page) {
    setState(() => _page = page);
    _load();
  }

  int get _pageCount =>
      _totalCount == 0 ? 1 : ((_totalCount + _pageSize - 1) ~/ _pageSize);

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Padding(
          padding: const EdgeInsets.fromLTRB(
            AppSpacing.screenPadding,
            AppSpacing.md,
            AppSpacing.screenPadding,
            AppSpacing.sm,
          ),
          child: TextField(
            controller: _searchController,
            textInputAction: TextInputAction.search,
            decoration: InputDecoration(
              hintText: 'Search by company name',
              prefixIcon: const Icon(Icons.search),
              suffixIcon: _searchController.text.isEmpty
                  ? null
                  : IconButton(
                      icon: const Icon(Icons.close),
                      onPressed: () {
                        _searchController.clear();
                        _search();
                      },
                    ),
            ),
            onChanged: (_) => setState(() {}),
            onSubmitted: (_) => _search(),
          ),
        ),
        Expanded(child: _buildList()),
        if (!_isLoading && _error == null && _pageCount > 1) _buildPager(),
      ],
    );
  }

  Widget _buildList() {
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
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.xl),
          child: Text(
            _error!,
            textAlign: TextAlign.center,
            style: TextStyle(color: Theme.of(context).colorScheme.error),
          ),
        ),
      );
    }

    if (_items.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.xl),
          child: Text(
            'No companies match that name.',
            style: Theme.of(context).textTheme.bodySmall,
          ),
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView.builder(
        padding: const EdgeInsets.fromLTRB(
          AppSpacing.screenPadding,
          0,
          AppSpacing.screenPadding,
          AppSpacing.md,
        ),
        itemCount: _items.length,
        itemBuilder: (context, index) {
          final company = _items[index];
          final rating = _ratings[company.id ?? -1];

          return Padding(
            padding: const EdgeInsets.only(bottom: AppSpacing.sm),
            child: CompanyTile(
              name: company.name ?? '',
              cityName: company.cityName,
              logoImageUrl: company.logoImageUrl,
              averageRating: rating?.averageRating,
              reviewCount: rating?.reviewCount,
              trailingLine:
                  '${Formats.money(company.startingPrice)} start · '
                  '${Formats.money(company.pricePerKm)} per km',
              onTap: company.id == null
                  ? null
                  : () => CompanyDetailScreen.open(context, company.id!),
            ),
          );
        },
      ),
    );
  }

  Widget _buildPager() {
    final theme = Theme.of(context);

    return Padding(
      padding: const EdgeInsets.fromLTRB(
        AppSpacing.screenPadding,
        0,
        AppSpacing.screenPadding,
        AppSpacing.md,
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          IconButton(
            tooltip: 'Previous page',
            icon: const Icon(Icons.chevron_left),
            onPressed: _page > 1 ? () => _goToPage(_page - 1) : null,
          ),
          Text('Page $_page of $_pageCount', style: theme.textTheme.bodySmall),
          IconButton(
            tooltip: 'Next page',
            icon: const Icon(Icons.chevron_right),
            onPressed: _page < _pageCount ? () => _goToPage(_page + 1) : null,
          ),
        ],
      ),
    );
  }
}

class CompanyTile extends StatelessWidget {
  const CompanyTile({
    super.key,
    required this.name,
    required this.cityName,
    required this.logoImageUrl,
    required this.averageRating,
    required this.reviewCount,
    this.trailingLine,
    this.onTap,
  });

  final String name;
  final String? cityName;
  final String? logoImageUrl;
  final double? averageRating;
  final int? reviewCount;
  final String? trailingLine;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Material(
      color: AppColors.surface,
      borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
        child: Container(
          padding: const EdgeInsets.all(AppSpacing.md),
          decoration: BoxDecoration(
            border: Border.all(color: AppColors.border),
            borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
          ),
          child: Row(
            children: [
              ApiImage.logo(url: logoImageUrl, size: 52),
              const SizedBox(width: AppSpacing.md),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      name,
                      style: theme.textTheme.titleMedium,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    if ((cityName ?? '').isNotEmpty)
                      Text(cityName!, style: theme.textTheme.bodySmall),
                    const SizedBox(height: AppSpacing.xs),
                    RatingStars(
                      average: averageRating,
                      reviewCount: reviewCount,
                    ),
                    if (trailingLine != null) ...[
                      const SizedBox(height: 2),
                      Text(
                        trailingLine!,
                        style: theme.textTheme.bodySmall,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                    ],
                  ],
                ),
              ),
              if (onTap != null)
                const Icon(Icons.chevron_right, color: AppColors.textMuted),
            ],
          ),
        ),
      ),
    );
  }
}
