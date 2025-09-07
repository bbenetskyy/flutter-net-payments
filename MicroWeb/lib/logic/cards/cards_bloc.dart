import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../data/models/responses/card_response.dart';
import '../../data/repositories/cards_repository.dart';

part 'cards_event.dart';
part 'cards_state.dart';

class CardsBloc extends Bloc<CardsEvent, CardsState> {
  CardsBloc(this._repo) : super(const CardsState.initial()) {
    on<CardsRequested>(_onRequested);
  }

  final CardsRepository _repo;

  Future<void> _onRequested(CardsRequested event, Emitter<CardsState> emit) async {
    emit(state.copyWith(loading: true, error: null));
    try {
      final data = await _repo.listCards();
      final list = CardResponse.listFromJson(data);
      emit(state.copyWith(loading: false, items: list, error: null));
    } catch (e) {
      emit(state.copyWith(loading: false, error: e.toString()));
    }
  }
}
