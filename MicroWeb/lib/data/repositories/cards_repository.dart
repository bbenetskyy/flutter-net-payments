import '../models/requests/assign_card_request.dart';
import '../models/requests/create_card_request.dart';
import '../models/requests/update_card_request.dart';
import '../models/top_up_request.dart';

/// Abstraction for Cards service REST calls routed via API Gateway base URL.
abstract class CardsRepository {
  // Cards CRUD
  Future<dynamic> listCards();
  Future<dynamic> getCardById(String id);
  Future<dynamic> createCard(CreateCardRequest request);
  Future<dynamic> updateCard(String id, UpdateCardRequest request);
  Future<void> terminateCard(String id);

  // Actions
  Future<void> assignCard(String id, AssignCardRequest request);
  Future<void> topUpCard(String id, TopUpRequest request);
}
