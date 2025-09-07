// import 'package:flutter_bloc/flutter_bloc.dart';
//
// import '../../data/repositories/cards_repository.dart';
// import 'cards_bloc.dart';
//
// class CardsCubit extends Cubit<CardsState> {
//   CardsCubit(this._cardsRepository) : super(const CardsState()) {
//     loadCards();
//   }
//
//   final CardsRepository _cardsRepository;
//
//   Future<void> loadCards() async {
//     emit(state.copyWith(loading: true));
//     try {
//       final cards = await _cardsRepository.getCards();
//       emit(state.copyWith(items: cards, loading: false));
//     } catch (e) {
//       emit(state.copyWith(loading: false, error: e.toString()));
//     }
//   }
// }
