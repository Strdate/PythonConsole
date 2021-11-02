from __future__ import annotations

import abc
from typing import Any, Dict, List, Optional, Type, TypeVar

from . import header, utils, api

T = TypeVar('T', bound='BaseObject')

class BaseObject(abc.ABC):
    _attrs: Dict[str, Any]

    def __init__(self, game: api.Game, *, **kwargs):
        self._game = game
        self._attrs = {}
        self._attrs.update(kwargs)

    @classmethod
    def from_message(cls: Type[T], message: header.BaseMessage) -> T:
        ret = super().__new__(cls)
        ret.__init__(**message.content)
        return ret

    def update(self, message: header.BaseMessage) -> None:
        self._attrs.update(message.content)


class NaturalResourceCell(BaseObject):

    @staticmethod
    def clamp(val: int, min_: int, max_: int) -> int:
        if min_ > max_:
            raise ValueError('min > max')
        if (min_ > val):
            return min_
        if (max_ < val):
            return max_
        return val

    @classmethod
    def from_pos(cls, game: api.Game, pos: utils.Vector) -> 'NaturalResourceCell':
        rowID = cls.clamp(int(pos.x / 33.75 + 256), 0, 511)
        columnID = cls.clamp(int(pos.z / 33.75 + 256), 0, 511)
        ret = cls(game, rowID=rowID, columnID=columnID)
        ret.update(ret.fetch_data())
        return ret
        
    @abc.abstractmethod
    def fetch_data(self) -> header.BaseMessage:
        return self._game._get_resource(self.id_)

    @property
    def rowID(self) -> int:
        return self._attrs['rowID']

    @property
    def columnID(self) -> int:
        return self._attrs['columnID']

    @property
    def water(self) -> int:
        return self._attrs['water']

    @property
    def pollution(self) -> int:
        return self._attrs['pollution']

    @pollution.setter
    def pollution(self, pollution: int) -> None:
        self._game._set_resource(
            self.id_, 'pollution', pollution
        )

    @property
    def fertility(self) -> int:
        return self._attrs['fertility']

    @fertility.setter
    def fertility(self, fertility: int) -> None:
        self._game._set_resource(
            self.id_, 'fertility', fertility
        )
    
    @property
    def forest(self) -> int:
        return self._attrs['forest']

    @forest.setter
    def forest(self, forest: int) -> None:
        self._game._set_resource(
            self.id_, 'forest', forest
        )

    @property
    def ore(self) -> int:
        return self._attrs['ore']

    @ore.setter
    def ore(self, ore: int) -> None:
        self._game._set_resource(
            self.id_, 'ore', ore
        )

    @property
    def oil(self) -> int:
        return self._attrs['oil']

    @oil.setter
    def oil(self, oil: int) -> None:
        self._game._set_resource(
            self.id_, 'oil', oil
        )

    @property
    def id_(self) -> int:
        return self.rowID * 512 + self.columnID

class Point(utils.IPositionable):

    def __init__(self, game: api.Game, vector: utils.Vector):
        self._vector = vector
        self._game = game

    @property
    def type(self) -> str:
        return 'point'

    @property
    def position(self) -> utils.Vector:
        return self._vector
    
    @property
    @abc.abstractmethod
    def resources(self) -> NaturalResourceCell:
        return NaturalResourceCell.from_pos(self._game, self._vector)


class RenderableObjectHandle(BaseObject):

    @property
    def id_(self) -> int:
        return self._attrs['id']

    def delete(self) -> None:
        return self._game.remove_render_object(self.id_)

class GameObject(utils.IPositionable, BaseObject):

    @property
    def id_(self) -> int:
        return self._attrs['id']

    @property
    def position(self) -> utils.Vector:
        return self._attrs['position']

    @property
    @abc.abstractmethod
    def type(self) -> str:
        ...


class EntityObject(GameObject):

    @property
    def exists(self) -> bool:
        return self._attrs['exists']

    @property
    def deleted(self) -> bool:
        return not self.exists

    @property
    def position(self) -> utils.Vector:
        return self._attrs['position']

    @position.setter
    def position(self, pos: utils.Vector) -> None:
        self.move(pos)

    @property
    def prefab_name(self) -> str:
        return self._attrs['prefab_name']

    def delete(self):
        return self._game._delete_obj(self.id_, self.type)

    def move(self, pos: utils.IPositionable):
        return self._game._move_obj(self.id_, self.type, pos)

    def refresh(self):
        self.update(self._game._get_obj(id_=self.id_, type_=self.type))


class RotatableEntity(EntityObject):

    @property
    def angle(self) -> float:
        return self._attrs['angle']

    @angle.setter
    def angle(self, angle: float) -> None:
        self.move(self.position, angle)

    def move(self, pos: utils.IPositionable, angle: float):
        return self._game._move_obj(self.id_, self.type, pos, angle)


class NetPrefab(BaseObject):

    @property
    def type(self) -> str:
        return "net prefab"

    @property
    def id_(self) -> str:
        return self._attrs['id']

    @property
    def name(self) -> str:
        return self.id_

    @property
    def width(self) -> float:
        return self._attrs['width']

    @property
    def is_overground(self) -> bool:
        return self._attrs['is_overground']

    @property
    def is_underground(self) -> bool:
        return self._attrs['is_underground']

    @property
    def fw_vehicle_lane_count(self) -> int:
        return self._attrs['fw_vehicle_lane_count']

    @property
    def bw_vehicle_lane_count(self) -> int:
        return self._attrs['bw_vehicle_lane_count']


class NetworkObject(EntityObject):

    @property
    def prefab(self) -> NetPrefab:
        return self._game.get_net_prefab(self.prefab_name)


class Tree(EntityObject):

    @property
    def type(self) -> str:
        return 'tree'


class Building(RotatableEntity):

    @property
    def type(self) -> str:
        return 'building'


class Prop(RotatableEntity):

    @property
    def type(self) -> str:
        return 'prop'


class Node(NetworkObject):

    @property
    def type(self) -> str:
        return 'node'

    @property
    def terrain_offset(self) -> float:
        return self._attrs['terrain_offset']

    @property
    def building_id(self) -> int:
        return self._attrs['building_id']

    @property
    def seg_count(self) -> int:
        return self._attrs['seg_count']

    @property
    def building(self) -> Optional[Building]:
        if self.building_id:
            return self._game.get_building(id_=self.building_id)
        else:
            return None

    @property
    def segments(self) -> List['Segment']:
        return self._game._get_adj_segments(self.id_)


class Segment(NetworkObject):

    @property
    def type(self) -> str:
        return 'segment'

    @property
    def start_node_id(self) -> int:
        return self._attrs['start_node_id']

    @property
    def end_node_id(self) -> int:
        return self._attrs['end_node_id']

    @property
    def start_dir(self) -> utils.Vector:
        return self._attrs['start_dir']

    @property
    def end_dir(self) -> utils.Vector:
        return self._attrs['end_dir']

    @property
    def bezier(self) -> utils.Bezier:
        return self._attrs['bezier']

    @property
    def length(self) -> float:
        return self._attrs['length']

    @property
    def is_straight(self) -> bool:
        return self._attrs['is_straight']

    @property
    def start_node(self) -> Node:
        return self._game.get_node(self.start_node_id)

    @property
    def end_node(self) -> Node:
        return self._game.get_node(self.end_node_id)

    def get_other_node(self, node: Node | int) -> Node:
        if isinstance(node, Node):
            node = node.id_
        if node == self.start_node_id:
            return self.end_node
        else:
            return self.start_node

class PathBuilder():

    def __init__(self, game: api.Game,
        start_node: utils.IPositionable,
        options: Optional[str | utils.NetOptions] = None
    ):
        if options is None:
            if isinstance(start_node, Node):
                options = start_node.prefab_name
            else:
                raise ValueError(
                    "You must provide prefab name or NetOptions as the second "
                    "param, if that connot be inferred from first param"
                )

        self._game = game
        self.options = utils.NetOptions.ensure(options)
        self._last_position = start_node
        self._last_segments: List[Segment] = []
        self._segments: List[Segment] = []

    @property
    def last_segments(self) -> List[Segment]:
        return self._last_segments

    @property
    def segments(self) -> List[Segment]:
        return self._segments
        self._last_position = value

    @property
    def first_node(self) -> Node:
        return self.segments[0].start_node

    @property
    def last_node(self) -> Node:
        return self.segments[-1].end_node

    def _create_segments(self,
        end_node: utils.IPositionable,
        options: Any, start_dir: Optional[utils.Vector],
        end_dir: Optional[utils.Vector], middle_pos: Optional[utils.Vector]
    ) -> List[Segment]:
        ret = self._game.create_segments(
            self.last_node.position, end_node, options,
            start_dir=start_dir, end_dir=end_dir, control_point=middle_pos,
        )
        assert ret is not None
        return ret

    def _create_path(
        self, end_node: utils.IPositionable, options: Any,
        start_dir: Optional[utils.Vector], end_dir: Optional[utils.Vector],
        middle_pos: Optional[utils.Vector]
    ):
        self.last_segments.clear()
        self.last_segments.extend(self._create_segments(
            end_node, options, start_dir, end_dir, middle_pos
        ))
        self.segments.extend(self.last_segments)

    def path_to(self,
        end_node: utils.IPositionable, *,
        start_dir: Optional[utils.Vector] = None,
        end_dir: Optional[utils.Vector] = None,
        middle_pos: Optional[utils.Vector] = None
    ):
        if (start_dir is None) ^ (end_dir is None):
            raise ValueError("start_dir and end_dir must be both set or both unset")
        if (middle_pos is None) == (start_dir is None):
            raise ValueError("middle_pos and start_dir can't be both present")
        
        self._create_path(
            end_node, self.options, start_dir, end_dir, middle_pos
        )
