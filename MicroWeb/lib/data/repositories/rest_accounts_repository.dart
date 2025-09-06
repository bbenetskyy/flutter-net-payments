import '../services/api_client.dart';
import 'accounts_repository.dart';

/// Accounts service repository calling endpoints via the API Gateway base URL.
class RestAccountsRepository implements AccountsRepository {
  RestAccountsRepository(this._api);
  final ApiClient _api;

  @override
  Future<dynamic> listMyAccounts({Map<String, dynamic>? query}) async {
    return await _api.get('/accounts/my', query: query);
  }
}
