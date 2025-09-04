import '../models/requests/assign_card_request.dart';
import '../models/requests/create_card_request.dart';
import '../models/top_up_request.dart';
import '../models/requests/update_card_request.dart';
import '../services/api_client.dart';
import 'cards_repository.dart';

/// Cards service repository calling endpoints via the API Gateway base URL.
class RestCardsRepository implements CardsRepository {
  RestCardsRepository(this._api);
  final ApiClient _api;

  @override
  Future<dynamic> listCards() async {
    return await _api.get('/cards');
  }

  @override
  Future<dynamic> getCardById(String id) async {
    return await _api.get('/cards/$id');
  }

  @override
  Future<dynamic> createCard(CreateCardRequest request) async {
    return await _api.post('/cards', body: request.toJson());
  }

  @override
  Future<dynamic> updateCard(String id, UpdateCardRequest request) async {
    return await _api.put('/cards/$id', body: request.toJson());
  }

  @override
  Future<void> deleteCard(String id) async {
    await _api.delete('/cards/$id');
  }

  @override
  Future<void> assignCard(String id, AssignCardRequest request) async {
    await _api.post('/cards/$id/assign', body: request.toJson());
  }

  @override
  Future<void> topUpCard(String id, TopUpRequest request) async {
    await _api.post('/cards/$id/top-up', body: request.toJson());
  }
}
