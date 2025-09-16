import '../models/requests/create_account_request.dart';
import '../models/responses/account_response.dart';
import '../services/api_client.dart';
import 'accounts_repository.dart';

/// Accounts service repository calling endpoints via the API Gateway base URL.
class RestAccountsRepository implements AccountsRepository {
  RestAccountsRepository(this._api);
  final ApiClient _api;

  @override
  Future<List<AccountResponse>> listMyAccounts({Map<String, dynamic>? query}) async {
    final data = await _api.get('/accounts/my', query: query);
    return AccountResponse.listFromJson(data);
  }

  @override
  Future<List<AccountResponse>> listUserAccounts(String userId, {Map<String, dynamic>? query}) async {
    final data = await _api.get('/accounts/$userId', query: query);
    return AccountResponse.listFromJson(data);
  }

  @override
  Future<AccountResponse> createAccount(CreateAccountRequest request) async {
    final data = await _api.post('/accounts', body: request.toJson());
    // Accept either {account: {...}} or direct object
    if (data is Map && data['account'] is Map) {
      return AccountResponse.fromJson(Map<String, dynamic>.from(data['account'] as Map));
    }
    return AccountResponse.fromJson(Map<String, dynamic>.from(data as Map));
  }

  @override
  Future<void> deleteAccount(String id) async {
    await _api.delete('/accounts/$id');
  }
}
